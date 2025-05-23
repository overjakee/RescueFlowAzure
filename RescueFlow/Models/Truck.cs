using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RescueFlow.Models
{
    public class Truck
    {
        [Key]
        [Required(ErrorMessage = "ต้องใส่ค่า TruckId มาด้วย")]
        public string TruckId { get; set; } = default!;

        [NotMapped]
        public Dictionary<string, int> AvailableResources { get; set; } = new();

        [JsonIgnore]
        public string AvailableResourcesJson
        {
            get => JsonSerializer.Serialize(AvailableResources);
            set => AvailableResources = string.IsNullOrEmpty(value)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(value)!;
        }

        [NotMapped]
        public Dictionary<string, int> TravelTimeToArea { get; set; } = new();

        [JsonIgnore]
        public string TravelTimeToAreaJson
        {
            get => JsonSerializer.Serialize(TravelTimeToArea);
            set => TravelTimeToArea = string.IsNullOrEmpty(value)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(value)!;
        }
    }
}
