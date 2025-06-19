namespace RescueFlow.DTO.Area.Request
{
    public class SearchAreaRequest
    {
        public int? urgencyLevel { get; set; } = 0;
        public string resourceName { get; set; }
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
