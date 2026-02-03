using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum MonitoringSystemTypeEnum
    {
        [Display(Name = "Created")]
        CREATED = 1,

        [Display(Name = "Deleted")]
        DELETED = 2,

        [Display(Name = "Changed Password")]
        CHANGED_PASSWORD = 3
    }
}
