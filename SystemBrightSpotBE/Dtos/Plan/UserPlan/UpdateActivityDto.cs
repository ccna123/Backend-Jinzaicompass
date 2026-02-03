using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlan
{
    public class UpdateActivityDto
    {
        [JsonPropertyOrder(-2)]
        [JsonPropertyName("comment")]
        [MaxLength(255, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "UpdateCommentMaxLength")]
        public string? comment { get; set; }

        [JsonPropertyOrder(-1)]
        [JsonPropertyName("type")]
        [Required]
        [EnumDataType(typeof(ActivityStatusEnum))]
        public ActivityStatusEnum type { get; set; }
    }
}
