using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum StatusHistoryTypeEnum
    {
        [Display(Name = "Join")]
        Join = 1,

        [Display(Name = "OnLeave")]
        OnLeave = 2,

        [Display(Name = "Return")]
        Return = 3,

        [Display(Name = "Resign")]
        Resign = 4,
    }
}
