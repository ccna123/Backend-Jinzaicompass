using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum OrganizationTypeEnum
    {
        [Display(Name = "Department")]
        department = 1,

        [Display(Name = "Division")]
        division = 2,

        [Display(Name = "Group")]
        group = 3
    }
}
