using RescueFlow.Data;
using RescueFlow.DTO.Area.Request;
using RescueFlow.DTO.Area.Response;
using RescueFlow.Interfaces;
using RescueFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace RescueFlow.Services
{
    public class AreaService : IAreaService
    {
        private readonly RescueFlowDbContext _context;
        public AreaService(RescueFlowDbContext context)
        {
            _context = context;
        }

        public async Task<AddAreaResponse> AddArea(AddAreaRequest request)
        {
            ValidateAddAreaRequest(request);

            var exists = await _context.Areas.FirstOrDefaultAsync(a => a.AreaId == request.AreaId);
            if (exists != null)
                throw new InvalidOperationException($"ข้อมูลของ AreaId '{request.AreaId}' มีอยู่แล้ว");

            var area = new Area
            {
                AreaId = request.AreaId,
                UrgencyLevel = request.UrgencyLevel,
                RequiredResources = request.RequiredResources,
                TimeConstraintHours = request.TimeConstraintHours
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            var response = new AddAreaResponse
            {
                AreaId = area.AreaId,
                UrgencyLevel = area.UrgencyLevel,
                RequiredResources = area.RequiredResources,
                TimeConstraintHours = area.TimeConstraintHours
            };
            return response;
        }

        public async Task<List<GetAreaResponse>> GetAreas()
        {
            var areas = await _context.Areas.ToListAsync();

            var response = areas.Select(a => new GetAreaResponse
            {
                AreaId = a.AreaId,
                UrgencyLevel = a.UrgencyLevel,
                RequiredResources = a.RequiredResources,
                TimeConstraintHours = a.TimeConstraintHours
            }).ToList();

            return response;
        }

        public async Task<GetAreaResponse> GetAreasById(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
                throw new ArgumentException("AreaId ต้องมีข้อมูล");

            var area = await _context.Areas.FirstOrDefaultAsync(a => a.AreaId == areaId);
            if (area == null)
                throw new KeyNotFoundException($"ไม่พบข้อมูล AreaId '{areaId}'");

            var response = new GetAreaResponse
            {
                AreaId = area.AreaId,
                UrgencyLevel = area.UrgencyLevel,
                RequiredResources = area.RequiredResources,
                TimeConstraintHours = area.TimeConstraintHours
            };

            return response;
        }

        public async Task<UpdateAreaResponse> UpdateArea(UpdateAreaRequest request, string areaId)
        {
            ValidateUpdateAreaRequest(request, areaId);

            var existingArea = await _context.Areas.FirstOrDefaultAsync(a => a.AreaId == request.AreaId);
            if (existingArea == null)
                throw new InvalidOperationException($"ไม่พบข้อมูล AreaId '{request.AreaId}' ในระบบ");

            existingArea.UrgencyLevel = request.UrgencyLevel;
            existingArea.RequiredResources = request.RequiredResources;
            existingArea.TimeConstraintHours = request.TimeConstraintHours;

            await _context.SaveChangesAsync();

            var response = new UpdateAreaResponse
            {
                AreaId = existingArea.AreaId,
                UrgencyLevel = existingArea.UrgencyLevel,
                RequiredResources = existingArea.RequiredResources,
                TimeConstraintHours = existingArea.TimeConstraintHours
            };

            return response;
        }

        public async Task DeleteAreaById(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
                throw new ArgumentException("AreaId ต้องมีข้อมูล");

            var area = await _context.Areas.FindAsync(areaId);
            if (area == null)
                throw new KeyNotFoundException($"ไม่พบ Area ที่มี ID '{areaId}'");

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();
        }


        private void ValidateAddAreaRequest(AddAreaRequest request)
        {
            if (string.IsNullOrEmpty(request.AreaId))
                throw new ArgumentException("AreaId ต้องมีข้อมูล");

            if (!(request.UrgencyLevel >= 1 && request.UrgencyLevel <= 5))
                throw new ArgumentException("UrgencyLevel ต้องมากกว่า 1 และ น้อยกว่า 5");

            if (request.RequiredResources == null || !request.RequiredResources.Any())
                throw new ArgumentException("RequiredResources ต้องมีข้อมูล");

            if (request.TimeConstraintHours <= 0)
                throw new ArgumentException("TimeConstraintHours ต้องมากว่า 0");
        }

        private void ValidateUpdateAreaRequest(UpdateAreaRequest request, string areaId)
        {
            if (string.IsNullOrEmpty(request.AreaId) || string.IsNullOrEmpty(areaId) || request.AreaId != areaId)
                throw new ArgumentException("AreaId ต้องมีข้อมูล และต้องตรงกัน");

            if (!(request.UrgencyLevel >= 1 && request.UrgencyLevel <= 5))
                throw new ArgumentException("UrgencyLevel ต้องมากกว่า 1 และ น้อยกว่า 5");

            if (request.RequiredResources == null || !request.RequiredResources.Any())
                throw new ArgumentException("RequiredResources ต้องมีข้อมูล");

            if (request.TimeConstraintHours <= 0)
                throw new ArgumentException("TimeConstraintHours ต้องมากว่า 0");
        }
    }
}
