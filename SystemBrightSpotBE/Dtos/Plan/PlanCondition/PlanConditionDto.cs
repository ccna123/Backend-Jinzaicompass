using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Plan.PlanCondition
{
    public class PlanConditionDto
    {
        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "ConditionNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "ConditionNameMaxLength")]
        public string name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "ConditionOverviewRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "ConditionOverviewMaxLength")]
        public string overview { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "EsttimeRequired")]
        [Range(1, 100, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "EsttimeRange")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "EsttimeRegx")]
        public int est_time { get; set; } = 1;
    }
}
