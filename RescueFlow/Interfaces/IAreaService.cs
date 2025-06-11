using RescueFlow.DTO.Area.Request;
using RescueFlow.DTO.Area.Response;

namespace RescueFlow.Interfaces
{
    public interface IAreaService
    {
        Task<List<GetAreaResponse>> GetAreas();
        Task<GetAreaResponse> GetAreasById(string areaId);
        Task<AddAreaResponse> AddArea(AddAreaRequest request);
        Task<UpdateAreaResponse> UpdateArea(UpdateAreaRequest request, string areaId);
        Task DeleteAreaById(string areaId);

    }
}
