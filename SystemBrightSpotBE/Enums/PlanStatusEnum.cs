using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum PlanStatusEnum
    {
        [Display(Name = "Completed")]
        COMPLETED = 1,

        [Display(Name = "In Progress")]
        IN_PROGRESS = 2,

        [Display(Name = "No Start")]
        NO_START = 3
    }
}
