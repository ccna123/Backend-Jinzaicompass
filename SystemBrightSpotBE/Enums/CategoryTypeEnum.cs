using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum CategoryTypeEnum
    {
        [Display(Name = "Role")]
        role = 1,

        [Display(Name = "Gender")]
        gender = 2,

        [Display(Name = "Position")]
        position = 3,

        [Display(Name = "Employment type")]
        employment_type = 4,

        [Display(Name = "Employment status")]
        employment_status = 5,

        [Display(Name = "Certification")]
        certification = 6,

        [Display(Name = "Company award")]
        company_award = 7,

        [Display(Name = "Participation process")]
        participation_process = 8,

        [Display(Name = "Participation position")]
        participation_position = 9,

        [Display(Name = "Report type")]
        report_type = 10
    }
}
