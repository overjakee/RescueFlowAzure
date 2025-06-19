using Newtonsoft.Json;
using RescueFlow.DTO.Area.Request;
using RescueFlow.DTO.Area.Response;
using RescueFlow.Interfaces;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;
using System.Security.Cryptography;
using System.Text;

namespace RescueFlow.Services
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IRedisCacheService _redisCacheService;

        public AreaService(
            IAreaRepository areaRepository,
            IRedisCacheService redisCacheService)
        {
            _areaRepository = areaRepository;
            _redisCacheService = redisCacheService;
        }

        public async Task<AddAreaResponse> AddArea(AddAreaRequest request)
        {
            ValidateAddAreaRequest(request);

            if (await _areaRepository.ExistsAsync(request.AreaId))
                throw new InvalidOperationException($"ข้อมูลของ AreaId '{request.AreaId}' มีอยู่แล้ว");

            var area = new Area
            {
                AreaId = request.AreaId,
                UrgencyLevel = request.UrgencyLevel,
                RequiredResources = request.RequiredResources,
                TimeConstraintHours = request.TimeConstraintHours
            };

            await _areaRepository.AddAsync(area);

            return new AddAreaResponse
            {
                AreaId = area.AreaId,
                UrgencyLevel = area.UrgencyLevel,
                RequiredResources = area.RequiredResources,
                TimeConstraintHours = area.TimeConstraintHours
            };
        }

        public async Task<List<GetAreaResponse>> GetAreas()
        {
            var areas = await _areaRepository.GetAllAsync();

            return areas.Select(a => new GetAreaResponse
            {
                AreaId = a.AreaId,
                UrgencyLevel = a.UrgencyLevel,
                RequiredResources = a.RequiredResources,
                TimeConstraintHours = a.TimeConstraintHours
            }).ToList();
        }

        public async Task<List<GetAreaResponse>> GetAreas(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ArgumentException("pageNumber และ pageSize ต้องมากกว่า 0");

            var areas = await _areaRepository.GetPagedAsync(pageNumber, pageSize);

            return areas.Select(a => new GetAreaResponse
            {
                AreaId = a.AreaId,
                UrgencyLevel = a.UrgencyLevel,
                RequiredResources = a.RequiredResources,
                TimeConstraintHours = a.TimeConstraintHours
            }).ToList();
        }

        public async Task<List<GetAreaResponse>> SearchAreas(SearchAreaRequest request)
        {
            if (request.pageNumber <= 0 || request.pageSize <= 0)
                throw new ArgumentException("PageNumber และ PageSize ต้องมากกว่า 0");

            //var cacheKey = GenerateSearchKey(request);
            //var cached = await _redisCacheService.GetCacheAsync(cacheKey);
            //if (cached != null)
            //{
            //    return JsonConvert.DeserializeObject<List<GetAreaResponse>>(cached)!;
            //}

            var query = await _areaRepository.GetAllAsync();

            var filtered = query
                .Where(a =>
                    (!request.urgencyLevel.HasValue || a.UrgencyLevel == request.urgencyLevel.Value) &&
                    (string.IsNullOrEmpty(request.resourceName) || a.RequiredResources.ContainsKey(request.resourceName))
                )
                .OrderBy(a => a.AreaId)
                .Skip((request.pageNumber - 1) * request.pageSize)
                .Take(request.pageSize)
                .Select(a => new GetAreaResponse
                {
                    AreaId = a.AreaId,
                    UrgencyLevel = a.UrgencyLevel,
                    RequiredResources = a.RequiredResources,
                    TimeConstraintHours = a.TimeConstraintHours
                }).ToList();

            //await _redisCacheService.SetCacheAsync(cacheKey, JsonConvert.SerializeObject(filtered), TimeSpan.FromMinutes(5));

            return filtered;
        }

        public async Task<GetAreaResponse> GetAreasById(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
                throw new ArgumentException("AreaId ต้องมีข้อมูล");

            var area = await _areaRepository.GetByIdAsync(areaId);
            if (area == null)
                throw new KeyNotFoundException($"ไม่พบข้อมูล AreaId '{areaId}'");

            return new GetAreaResponse
            {
                AreaId = area.AreaId,
                UrgencyLevel = area.UrgencyLevel,
                RequiredResources = area.RequiredResources,
                TimeConstraintHours = area.TimeConstraintHours
            };
        }

        public async Task<UpdateAreaResponse> UpdateArea(UpdateAreaRequest request, string areaId)
        {
            ValidateUpdateAreaRequest(request, areaId);

            var existingArea = await _areaRepository.GetByIdAsync(areaId);
            if (existingArea == null)
                throw new InvalidOperationException($"ไม่พบข้อมูล AreaId '{areaId}' ในระบบ");

            existingArea.UrgencyLevel = request.UrgencyLevel;
            existingArea.RequiredResources = request.RequiredResources;
            existingArea.TimeConstraintHours = request.TimeConstraintHours;

            await _areaRepository.UpdateAsync(existingArea);

            return new UpdateAreaResponse
            {
                AreaId = existingArea.AreaId,
                UrgencyLevel = existingArea.UrgencyLevel,
                RequiredResources = existingArea.RequiredResources,
                TimeConstraintHours = existingArea.TimeConstraintHours
            };
        }

        public async Task DeleteAreaById(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
                throw new ArgumentException("AreaId ต้องมีข้อมูล");

            var area = await _areaRepository.GetByIdAsync(areaId);
            if (area == null)
                throw new KeyNotFoundException($"ไม่พบ Area ที่มี ID '{areaId}'");

            await _areaRepository.DeleteAsync(area);
        }

        private void ValidateAddAreaRequest(AddAreaRequest request)
        {
            if (string.IsNullOrEmpty(request.AreaId))
                throw new ArgumentException("AreaId ต้องมีข้อมูล");

            if (!(request.UrgencyLevel >= 1 && request.UrgencyLevel <= 5))
                throw new ArgumentException("UrgencyLevel ต้องอยู่ระหว่าง 1 ถึง 5");

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
                throw new ArgumentException("UrgencyLevel ต้องอยู่ระหว่าง 1 ถึง 5");

            if (request.RequiredResources == null || !request.RequiredResources.Any())
                throw new ArgumentException("RequiredResources ต้องมีข้อมูล");

            if (request.TimeConstraintHours <= 0)
                throw new ArgumentException("TimeConstraintHours ต้องมากว่า 0");
        }

        private string GenerateSearchKey(SearchAreaRequest request)
        {
            var serialized = JsonConvert.SerializeObject(request);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(serialized));
            return "search_area:" + Convert.ToHexString(hashBytes);
        }
    }
}
