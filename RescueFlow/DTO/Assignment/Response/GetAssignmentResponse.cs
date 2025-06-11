namespace RescueFlow.DTO.Assignment.Response
{
    public class GetAssignmentResponse
    {
        public string? AreaId { get; set; }
        public string? TruckId { get; set; }
        public Dictionary<string, int>? ResourcesDelivered { get; set; }
        public string? Message { get; set; }
    }
}
