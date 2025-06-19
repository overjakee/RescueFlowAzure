using RescueFlow.DTO.Area.Request;
using RescueFlow.DTO.Area.Response;

namespace RescueFlow.Interfaces
{
    public interface IAreaService
    {
        Task<List<GetAreaResponse>> GetAreas();
        Task<List<GetAreaResponse>> GetAreas(int pageNumber, int pageSize);
        Task<List<GetAreaResponse>> SearchAreas(SearchAreaRequest request);
        Task<GetAreaResponse> GetAreasById(string areaId);
        Task<AddAreaResponse> AddArea(AddAreaRequest request);
        Task<UpdateAreaResponse> UpdateArea(UpdateAreaRequest request, string areaId);
        Task DeleteAreaById(string areaId);

    }
}
