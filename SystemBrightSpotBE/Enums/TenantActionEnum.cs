using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum TenantActionEnum
    {
        [Display(Name = "Active")]
        ACTIVE = 1,

        [Display(Name = "Suspend")]
        SUSPEND = 2,

        [Display(Name = "Renew")]
        RENEW = 3,

        [Display(Name = "Terminate")]
        TERMINATE = 4
    }
}
