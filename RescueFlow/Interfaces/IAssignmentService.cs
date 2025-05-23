using RescueFlow.Models;

namespace RescueFlow.Interfaces
{
    public interface IAssignmentService
    {
        // คำนวณข้อมูลสำหรับจัดสรรทรัพยากร
        List<Assignment> ProcessAssignments();
    }
}
