using RescueFlow.Models;

namespace RescueFlow.Interfaces.Repositories
{
    public interface ITruckRepository
    {
        Task<Truck?> GetByIdAsync(string truckId);
        Task<List<Truck>> GetAllAsync();
        Task AddAsync(Truck truck);
        Task UpdateAsync(Truck truck);
        Task DeleteAsync(Truck truck);
        Task<bool> ExistsAsync(string truckId);
    }
}
