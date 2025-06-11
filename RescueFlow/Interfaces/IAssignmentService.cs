using RescueFlow.DTO.Assignment.Response;
using RescueFlow.Models;

namespace RescueFlow.Interfaces
{
    public interface IAssignmentService
    {
        Task<List<ProcessAssignmentResponse>> ProcessAssignments();
        Task<List<GetAssignmentResponse>> GetAssignmentsFromRedis();
        Task DeleteAssignmentsFromRedis();
    }
}
