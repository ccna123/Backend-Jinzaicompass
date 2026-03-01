using System.Text.Json.Serialization;
using SystemBrightSpotBE.Dtos.Plan.PlanCondition;

namespace SystemBrightSpotBE.Dtos.Plan
{
    public class PlanDto
    {
        [JsonPropertyOrder(-8)]
        public long id { get; set; }
        [JsonPropertyOrder(-7)]
        public string name { get; set; } = String.Empty;
        [JsonPropertyOrder(-6)]
        public string description { get; set; } = String.Empty;
        [JsonPropertyOrder(-5)]
        public DateOnly start_date { get; set; }
        [JsonPropertyOrder(-4)]
        public DateOnly complete_date { get; set; }
        [JsonPropertyOrder(-3)]
        public DateTime? deleted_at { get; set; }
        [JsonPropertyOrder(-2)]
        public long? status { get; set; } = 3;
        [JsonPropertyOrder(-1)]
        public List<PlanConditionDto> conditions { get; set; } = new();
    }
}
