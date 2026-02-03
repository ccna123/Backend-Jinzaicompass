using System.Text.Json.Serialization;

namespace SystemBrightSpotBE.Dtos.Skill
{
    public class SkillDto
    {
        [JsonPropertyOrder(-2)]
        [JsonPropertyName("id")]
        public required long id { get; set; }

        [JsonPropertyOrder(-1)]
        [JsonPropertyName("name")]
        public required string name { get; set; }
    }
}
