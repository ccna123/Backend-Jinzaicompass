using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum UserPlanStatusEnum
    {
        [Display(Name = "Completed")]
        COMPLETED = 1,

        [Display(Name = "In Progress")]
        IN_PROGRESS = 2,

        [Display(Name = "Pending Approval")]
        PENDING_APPROVAL = 3
    }
}
