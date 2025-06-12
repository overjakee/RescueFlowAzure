using System.Text.Json;
using RescueFlow.DTO.Assignment.Response;
using RescueFlow.Interfaces;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;

namespace RescueFlow.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAreaRepository _areaRepository;
        private readonly ITruckRepository _truckRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IRedisCacheService _redisCacheService;

        private const string CACHE_KEY = "latest_assignments";

        public AssignmentService(
            IAreaRepository areaRepository,
            ITruckRepository truckRepository,
            IAssignmentRepository assignmentRepository,
            IRedisCacheService redisCacheService)
        {
            _areaRepository = areaRepository;
            _truckRepository = truckRepository;
            _assignmentRepository = assignmentRepository;
            _redisCacheService = redisCacheService;
        }

        public async Task<List<ProcessAssignmentResponse>> ProcessAssignments()
        {
            var areas = await _areaRepository.GetAllAsync();
            var originalTrucks = await _truckRepository.GetAllAsync();

            if (!areas.Any() || !originalTrucks.Any())
                throw new InvalidOperationException("ข้อมูลพื้นที่หรือรถบรรทุกไม่ครบถ้วน");

            var trucks = originalTrucks.Select(t => new Truck
            {
                TruckId = t.TruckId,
                TravelTimeToArea = new Dictionary<string, int>(t.TravelTimeToArea),
                AvailableResources = new Dictionary<string, int>(t.AvailableResources)
            }).ToList();

            var response = new List<ProcessAssignmentResponse>();
            var sortedAreas = areas.OrderByDescending(a => a.UrgencyLevel).ToList();

            foreach (var area in sortedAreas)
            {
                var truck = FindSuitableTruck(area, trucks);

                if (truck != null)
                {
                    AssignTruckToArea(area, truck, response);
                }
                else
                {
                    AddUnassignedArea(area, trucks, response);
                }
            }

            await _assignmentRepository.ClearAllAsync();

            var assignments = response.Select(r => new Assignment
            {
                AreaId = r.AreaId,
                TruckId = r.TruckId,
                ResourcesDelivered = r.ResourcesDelivered,
                Message = r.Message
            }).ToList();

            await _assignmentRepository.AddRangeAsync(assignments);

            var json = JsonSerializer.Serialize(assignments);
            await _redisCacheService.SetCacheAsync(CACHE_KEY, json, TimeSpan.FromMinutes(30));

            return response;
        }

        public async Task<List<GetAssignmentResponse>> GetAssignmentsFromRedis()
        {
            var cached = await _redisCacheService.GetCacheAsync(CACHE_KEY);

            if (!string.IsNullOrEmpty(cached))
            {
                var assignments = JsonSerializer.Deserialize<List<Assignment>>(cached, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (assignments != null)
                {
                    return assignments.Select(a => new GetAssignmentResponse
                    {
                        AreaId = a.AreaId,
                        TruckId = a.TruckId,
                        ResourcesDelivered = a.ResourcesDelivered,
                        Message = a.Message
                    }).ToList();
                }
            }

            throw new InvalidOperationException("ไม่มีข้อมูลการจัดสรรใน Redis หรือหมดอายุแล้ว");
        }

        public async Task DeleteAssignmentsFromRedis()
        {
            await _redisCacheService.DeleteCacheAsync(CACHE_KEY);
        }

        #region Private Functions

        private Truck? FindSuitableTruck(Area area, List<Truck> trucks)
        {
            foreach (var truck in trucks)
            {
                if (!truck.TravelTimeToArea.TryGetValue(area.AreaId, out var travelTime) || travelTime > area.TimeConstraintHours)
                    continue;

                if (!area.RequiredResources.All(req =>
                    truck.AvailableResources.TryGetValue(req.Key, out var available) && available >= req.Value))
                    continue;

                return truck;
            }

            return null;
        }

        private void AssignTruckToArea(Area area, Truck truck, List<ProcessAssignmentResponse> assignments)
        {
            foreach (var req in area.RequiredResources)
            {
                truck.AvailableResources[req.Key] -= req.Value;
            }

            assignments.Add(new ProcessAssignmentResponse
            {
                AreaId = area.AreaId,
                TruckId = truck.TruckId,
                ResourcesDelivered = new Dictionary<string, int>(area.RequiredResources)
            });
        }

        private void AddUnassignedArea(Area area, List<Truck> trucks, List<ProcessAssignmentResponse> assignments)
        {
            bool hasNoTravelInfo = trucks.All(t => !t.TravelTimeToArea.ContainsKey(area.AreaId));
            bool timeIssue = !trucks.Any(t =>
                t.TravelTimeToArea.TryGetValue(area.AreaId, out var travelTime) &&
                travelTime <= area.TimeConstraintHours);

            bool resourceIssue = !trucks.Any(t =>
                area.RequiredResources.All(req =>
                    t.AvailableResources.TryGetValue(req.Key, out var available) && available >= req.Value));

            bool missingResourceType = area.RequiredResources.Any(req =>
                !trucks.Any(t => t.AvailableResources.ContainsKey(req.Key)));

            string message;

            if (hasNoTravelInfo)
                message = "ไม่สามารถจัดสรรได้: ยังไม่มีการคำนวณระยะเวลาในการเดินทางสำหรับพื้นที่นี้";
            else if (missingResourceType)
                message = "ไม่สามารถจัดสรรได้: ไม่มีรถคันใดมีทรัพยากรประเภทที่ร้องขอ";
            else if (timeIssue && resourceIssue)
                message = "ไม่สามารถจัดสรรได้: เวลาเดินทางไม่พอ และทรัพยากรไม่เพียงพอ";
            else if (timeIssue)
                message = "ไม่สามารถจัดสรรได้: เวลาเดินทางไม่พอ";
            else if (resourceIssue)
                message = "ไม่สามารถจัดสรรได้: ทรัพยากรไม่เพียงพอ";
            else
                message = "ไม่สามารถจัดสรรทรัพยากรได้";

            assignments.Add(new ProcessAssignmentResponse
            {
                AreaId = area.AreaId,
                TruckId = null,
                ResourcesDelivered = null,
                Message = message
            });
        }

        #endregion
    }
}
