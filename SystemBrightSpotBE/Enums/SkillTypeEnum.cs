using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Enums
{
    public enum SkillTypeEnum
    {
        [Display(Name = "Experience job")]
        experience_job = 1,

        [Display(Name = "Experience field")]
        experience_field = 2,

        [Display(Name = "Experience area")]
        experience_area = 3,

        [Display(Name = "Specific skill")]
        specific_skill = 4
    }
}
