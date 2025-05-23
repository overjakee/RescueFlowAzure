using System.Linq;
using RescueFlow.Data;
using RescueFlow.Interfaces;
using RescueFlow.Models;

namespace RescueFlow.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly RescueFlowDbContext _context;

        public AssignmentService(RescueFlowDbContext context) 
        {
            _context = context;
        }

        public List<Assignment> ProcessAssignments()
        {
            try
            {
                var areas = _context.Areas.ToList();
                var originalTrucks = _context.Trucks.ToList();

                if (!areas.Any() || !originalTrucks.Any())
                {
                    throw new InvalidOperationException("ข้อมูลพื้นที่หรือรถบรรทุกไม่ครบถ้วน");
                }

                // clone ข้อมูลของ Truck ไม่ให้ EF นำข้อมูล Truck ไปอัพเดต
                var trucks = originalTrucks.Select(t => new Truck
                {
                    TruckId = t.TruckId,
                    TravelTimeToArea = new Dictionary<string, int>(t.TravelTimeToArea),
                    AvailableResources = new Dictionary<string, int>(t.AvailableResources)
                }).ToList();

                var assignments = new List<Assignment>();

                // เรียงลำดับข้อมูลความสำคัญ
                var sortedAreas = areas.OrderByDescending(a => a.UrgencyLevel).ToList();

                // ลูปหา Truck ที่เหมาะสมในแต่ละ Area
                foreach (var area in sortedAreas)
                {
                    var truck = FindSuitableTruck(area, trucks);

                    if (truck != null)
                    {
                        AssignTruckToArea(area, truck, assignments);
                    }
                    else
                    {
                        AddUnassignedArea(area, trucks, assignments);
                    }
                }

                return assignments;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("เกิดข้อผิดพลาดในการประมวลผลการมอบหมายทรัพยากร", ex);
            }
        }

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
        private void AssignTruckToArea(Area area, Truck truck, List<Assignment> assignments)
        {
            foreach (var req in area.RequiredResources)
            {
                truck.AvailableResources[req.Key] -= req.Value;
            }

            assignments.Add(new Assignment
            {
                AreaId = area.AreaId,
                TruckId = truck.TruckId,
                ResourcesDelivered = new Dictionary<string, int>(area.RequiredResources)
            });
        }

        // กรณีไม่สามารส่งได้ให้แจ้งว่าเพราะอะไร
        private void AddUnassignedArea(Area area, List<Truck> trucks, List<Assignment> assignments)
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

            assignments.Add(new Assignment
            {
                AreaId = area.AreaId,
                TruckId = null,
                ResourcesDelivered = null,
                Message = message
            });
        }
    }
}
