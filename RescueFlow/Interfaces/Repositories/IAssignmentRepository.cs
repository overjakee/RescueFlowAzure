using RescueFlow.Models;

namespace RescueFlow.Interfaces.Repositories
{
    public interface IAssignmentRepository
    {
        Task ClearAllAsync();
        Task AddRangeAsync(IEnumerable<Assignment> assignments);
    }

}
