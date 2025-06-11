namespace RescueFlow.DTO.Area.Request
{
    public class UpdateAreaRequest
    {
        public required string AreaId { get; set; }
        public int UrgencyLevel { get; set; }
        public Dictionary<string, int> RequiredResources { get; set; } = new();
        public int TimeConstraintHours { get; set; }
    }
}
