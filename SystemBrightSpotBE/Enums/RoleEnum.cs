using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum RoleEnum
    {
        [Display(Name = "System Admin")]
        SYSTEM_ADMIN = 1,

        [Display(Name = "Power User")]
        POWER_USER = 2,

        [Display(Name = "Senior User")]
        SENIOR_USER = 3,

        [Display(Name = "Contributor")]
        CONTRIBUTOR = 4,

        [Display(Name = "Member")]
        MEMBER = 5,

        [Display(Name = "Supper Admin")]
        SUPPER_ADMIN = 6,
    }
}
