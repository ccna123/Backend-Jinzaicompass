using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum UserPlanConditionStatusEnum
    {
        [Display(Name = "Completed")]
        COMPLETED = 1,

        [Display(Name = "In Complete")]
        IN_COMPLETE = 2,

        [Display(Name = "Pending Approval")]
        PENDING_APPROVAL = 3
    }
}
