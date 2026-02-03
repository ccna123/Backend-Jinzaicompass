using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum TenantStatusEnum
    {
        [Display(Name = "InPreview")]
        IN_PREVIEW = 1,

        [Display(Name = "Scheduled")]
        SCHEDULED = 2,

        [Display(Name = "Actived")]
        ACTIVED = 3,

        [Display(Name = "Suspended")]
        SUSPENDED = 4,

        [Display(Name = "Expired")]
        EXPIRED = 5,

        [Display(Name = "Renewal Due")]
        RENEWAL_DUE = 6
    }
}
