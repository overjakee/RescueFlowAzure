namespace RescueFlow.DTO.Truck.Response
{
    public class GetTruckResponse
    {
        public required string TruckId { get; set; }
        public Dictionary<string, int> AvailableResources { get; set; } = new();
        public Dictionary<string, int> TravelTimeToArea { get; set; } = new();
    }
}
