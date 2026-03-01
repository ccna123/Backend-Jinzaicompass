using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlan
{
    public class CreateActivityDto
    {
        [JsonPropertyOrder(-2)]
        [JsonPropertyName("comment")]
        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "CommentRequired")]
        [MaxLength(255, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "CommentMaxLength")]
        public string comment { get; set; } = String.Empty;

        [JsonPropertyOrder(-1)]
        [JsonPropertyName("type")]
        [Required]
        [EnumDataType(typeof(ActivityStatusEnum))]
        [DisallowEnumValues(ActivityStatusEnum.REVOKED)]
        public ActivityStatusEnum type { get; set; }
    }
}
