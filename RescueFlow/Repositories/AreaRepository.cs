using Microsoft.EntityFrameworkCore;
using RescueFlow.Data;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;

namespace RescueFlow.Repositories
{
    public class AreaRepository : IAreaRepository
    {
        private readonly RescueFlowDbContext _context;

        public AreaRepository(RescueFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Area?> GetByIdAsync(string areaId)
        {
            return await _context.Areas.FindAsync(areaId);
        }

        public async Task<List<Area>> GetAllAsync()
        {
            return await _context.Areas.ToListAsync();
        }

        public async Task AddAsync(Area area)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Area area)
        {
            _context.Areas.Update(area);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Area area)
        {
            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string areaId)
        {
            return await _context.Areas.AnyAsync(a => a.AreaId == areaId);
        }
    }
}
