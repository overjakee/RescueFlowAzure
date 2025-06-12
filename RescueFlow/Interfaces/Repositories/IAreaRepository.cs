using RescueFlow.Models;

namespace RescueFlow.Interfaces.Repositories
{
    public interface IAreaRepository
    {
        Task<Area?> GetByIdAsync(string areaId);
        Task<List<Area>> GetAllAsync();
        Task AddAsync(Area area);
        Task UpdateAsync(Area area);
        Task DeleteAsync(Area area);
        Task<bool> ExistsAsync(string areaId);
    }
}
