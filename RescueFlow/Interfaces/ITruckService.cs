using RescueFlow.DTO.Truck.Request;
using RescueFlow.DTO.Truck.Response;

namespace RescueFlow.Interfaces
{
    public interface ITruckService
    {
        Task<List<GetTruckResponse>> GetTrucks();
        Task<GetTruckResponse> GetTruckById(string TruckId);
        Task<AddTruckResponse> AddTruck(AddTruckRequest request);
        Task<UpdateTruckResponse> UpdateTruck(UpdateTruckRequest request, string TruckId);
        Task DeleteTruckById(string TruckId);
    }
}
