using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class SpecificSkill : BaseCategoryEntityModel
    {
        [Required]
        [ForeignKey("ExperienceArea")]
        public long experience_area_id { get; set; }
        public ExperienceArea? ExperienceArea { get; set; }
    }
}
