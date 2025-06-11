using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RescueFlow.Models
{
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; } 

        public string? AreaId { get; set; }
        public string? TruckId { get; set; }

        [NotMapped]
        public Dictionary<string, int>? ResourcesDelivered { get; set; }

        [JsonIgnore]
        public string? ResourcesDeliveredJson
        {
            get => ResourcesDelivered == null ? null : JsonSerializer.Serialize(ResourcesDelivered);
            set => ResourcesDelivered = string.IsNullOrEmpty(value)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, int>>(value);
        }

        public string? Message { get; set; }
    }
}
