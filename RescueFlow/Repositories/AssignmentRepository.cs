using Microsoft.EntityFrameworkCore;
using RescueFlow.Data;
using RescueFlow.Interfaces.Repositories;
using RescueFlow.Models;

namespace RescueFlow.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly RescueFlowDbContext _context;

        public AssignmentRepository(RescueFlowDbContext context)
        {
            _context = context;
        }

        public async Task ClearAllAsync()
        {
            var allAssignments = await _context.Assignments.ToListAsync();
            _context.Assignments.RemoveRange(allAssignments);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Assignment> assignments)
        {
            await _context.Assignments.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();
        }
    }
}
