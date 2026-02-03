using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum ActivityStatusEnum
    {
        [Display(Name = "Accepted")]
        ACCEPTED = 1,

        [Display(Name = "Submitted")]
        SUBMITTED = 2,

        [Display(Name = "Rejected")]
        REJECTED = 3,

        [Display(Name = "Revoked")]
        REVOKED = 4
    }
}
