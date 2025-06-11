using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RescueFlow.Data;
using RescueFlow.DTO.Assignment.Response;
using RescueFlow.Interfaces;
using RescueFlow.Models;

namespace RescueFlow.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly RescueFlowDbContext _context;
        private readonly IRedisCacheService _redisCacheService;

        private const string CACHE_KEY = "latest_assignments";
        public AssignmentService(
            RescueFlowDbContext context,
            IRedisCacheService redisCacheService)
        {
            _context = context;
            _redisCacheService = redisCacheService;
        }
        public async Task<List<ProcessAssignmentResponse>> ProcessAssignments()
        {
            var areas = await _context.Areas.ToListAsync();
            var originalTrucks = await _context.Trucks.ToListAsync();

            if (!areas.Any() || !originalTrucks.Any())
            {
                throw new InvalidOperationException("ข้อมูลพื้นที่หรือรถบรรทุกไม่ครบถ้วน");
            }

            var trucks = originalTrucks.Select(t => new Truck
            {
                TruckId = t.TruckId,
                TravelTimeToArea = new Dictionary<string, int>(t.TravelTimeToArea),
                AvailableResources = new Dictionary<string, int>(t.AvailableResources)
            }).ToList();

            var response = new List<ProcessAssignmentResponse>();

            // เรียงลำดับข้อมูลความสำคัญ
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

            _context.Assignments.RemoveRange(_context.Assignments);

            var assignments = response.Select(r => new Assignment
            {
                AreaId = r.AreaId,
                TruckId = r.TruckId,
                ResourcesDelivered = r.ResourcesDelivered,
                Message = r.Message
            }).ToList();

            _context.Assignments.AddRange(assignments);
            await _context.SaveChangesAsync();

            var json = JsonSerializer.Serialize(assignments);
            await _redisCacheService.SetCacheAsync(CACHE_KEY, json, TimeSpan.FromMinutes(30));

            return response;
        }
        public async Task<List<GetAssignmentResponse>> GetAssignmentsFromRedis()
        {
            var cached = await _redisCacheService.GetCacheAsync(CACHE_KEY);
            if (!string.IsNullOrEmpty(cached))
            {
                var assignment = JsonSerializer.Deserialize<List<Assignment>>(cached, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (assignment != null)
                {
                    var result = assignment.Select(a => new GetAssignmentResponse
                    {
                        AreaId = a.AreaId,
                        TruckId = a.TruckId,
                        ResourcesDelivered = a.ResourcesDelivered,
                        Message = a.Message
                    }).ToList();

                    return result;
                }
            }
            else
            {
                throw new InvalidOperationException("ไม่มีข้อมูลการจัดสรรใน Redis หรือหมดอายุแล้ว");
            }

            return new List<GetAssignmentResponse>();
        }
        public async Task DeleteAssignmentsFromRedis()
        {
            await _redisCacheService.DeleteCacheAsync(CACHE_KEY);
        }

        #region Private Functions

        // ตรวจสอบว่ามีทรัพยากรและเวลาพอหรือเปล่า
        private Truck? FindSuitableTruck(Area area, List<Truck> trucks)
        {
            foreach (var truck in trucks)
            {
                // มีอย่างใดอย่างหนึ่งไม่พอไปเช็คตัวอื่นต่อ
                // เวลาพอไหม
                if (!truck.TravelTimeToArea.TryGetValue(area.AreaId, out var travelTime) ||
                    travelTime > area.TimeConstraintHours)
                {
                    continue;
                }

                // จำหน่วยทรัพยากรพอไหม
                if (!area.RequiredResources.All(req =>
                    truck.AvailableResources.TryGetValue(req.Key, out var available) &&
                    available >= req.Value))
                {
                    continue;
                }

                return truck;
            }

            return null;
        }

        // สร้างข้อมูลสำหรับแจ้งว่า รถคันไหนไปส่งที่ไหนรายการอะไรบ้าง
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

        // กรณีไม่สามารส่งได้ให้แจ้งว่าเพราะอะไร
        private void AddUnassignedArea(Area area, List<Truck> trucks, List<ProcessAssignmentResponse> assignments)
        {
            // หาว่ามีคำนวณระยะเวลาในเส้นทางที่ระบุหรือยัง
            bool hasNoTravelInfo = trucks.All(t => !t.TravelTimeToArea.ContainsKey(area.AreaId));

            // เช็คว่าเวลาในการเดินทางพอหรือไม่
            bool timeIssue = !trucks.Any(t =>
                t.TravelTimeToArea.TryGetValue(area.AreaId, out var travelTime) &&
                travelTime <= area.TimeConstraintHours);

            // เช็คว่าทรัพยากรพอหรือไม่
            bool resourceIssue = !trucks.Any(t =>
                area.RequiredResources.All(req =>
                    t.AvailableResources.TryGetValue(req.Key, out var available) &&
                    available >= req.Value));

            // ตรวจสอบว่าทรัพยากรประเภทที่ต้องการมีในรถหรือไม่
            bool missingResourceType = area.RequiredResources.Any(req =>
                !trucks.Any(t => t.AvailableResources.ContainsKey(req.Key)));

            string message;

            if (hasNoTravelInfo)
            {
                message = "ไม่สามารถจัดสรรได้: ยังไม่มีการคำนวณระยะเวลาในการเดินทางสำหรับพื้นที่นี้";
            }
            else if (missingResourceType)
            {
                message = "ไม่สามารถจัดสรรได้: ไม่มีรถคันใดมีทรัพยากรประเภทที่ร้องขอ";
            }
            else if (timeIssue && resourceIssue)
            {
                message = "ไม่สามารถจัดสรรได้: เวลาเดินทางไม่พอ และทรัพยากรไม่เพียงพอ";
            }
            else if (timeIssue)
            {
                message = "ไม่สามารถจัดสรรได้: เวลาเดินทางไม่พอ";
            }
            else if (resourceIssue)
            {
                message = "ไม่สามารถจัดสรรได้: ทรัพยากรไม่เพียงพอ";
            }
            else
            {
                message = "ไม่สามารถจัดสรรทรัพยากรได้";
            }

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
