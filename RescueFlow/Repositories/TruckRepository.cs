using Microsoft.EntityFrameworkCore;
using RescueFlow.Data;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;

namespace RescueFlow.Repositories
{
    public class TruckRepository : ITruckRepository
    {
        private readonly RescueFlowDbContext _context;

        public TruckRepository(RescueFlowDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string truckId)
        {
            return await _context.Trucks.AnyAsync(t => t.TruckId == truckId);
        }

        public async Task<Truck?> GetByIdAsync(string truckId)
        {
            return await _context.Trucks.FirstOrDefaultAsync(t => t.TruckId == truckId);
        }

        public async Task<List<Truck>> GetAllAsync()
        {
            return await _context.Trucks.ToListAsync();
        }

        public async Task AddAsync(Truck truck)
        {
            _context.Trucks.Add(truck);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Truck truck)
        {
            _context.Trucks.Update(truck);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Truck truck)
        {
            _context.Trucks.Remove(truck);
            await _context.SaveChangesAsync();
        }
    }
}
