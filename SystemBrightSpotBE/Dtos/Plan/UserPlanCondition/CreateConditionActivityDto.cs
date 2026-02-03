using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlanCondition
{
    public class CreateConditionActivityDto : CreateActivityDto
    {
        [AllowedContentTypeAttribute(new[] { "image/jpeg", "image/png", "image/svg+xml" }, MaxWidth = 1920, MaxHeight = 1080, MaxSize = 5 )]
        public IFormFile? file { get; set; }
    }
}
