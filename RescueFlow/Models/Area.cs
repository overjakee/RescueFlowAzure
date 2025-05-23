using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RescueFlow.Models
{
    public class Area
    {
        [Key]
        [Required(ErrorMessage = "กรุณากรอก AreaId มาด้วย")]
        public string AreaId { get; set; } = default!;

        [Range(1, 5 ,ErrorMessage = "กรุณากรอกในช่วง 1 ถึง 5")]
        public int UrgencyLevel { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "กรุณากรอกรายการทรัพยากรที่ต้องการอย่างน้อย 1 รายการ")]
        public Dictionary<string, int> RequiredResources { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "กรุณากรอกค่ามากกว่าหรือเท่ากับ 1 ชั่วโมง")]
        public int TimeConstraintHours { get; set; }

        [JsonIgnore]
        public string RequiredResourcesJson
        {
            get => JsonSerializer.Serialize(RequiredResources);
            set => RequiredResources = string.IsNullOrEmpty(value) ? new() : JsonSerializer.Deserialize<Dictionary<string, int>>(value);
        }
    }
}
