using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ExperienceArea : BaseCategoryEntityModel
    {
        [Required]
        [ForeignKey("ExperienceField")]
        public long experience_field_id { get; set; }
        public ExperienceField? ExperienceField { get; set; }

        public ICollection<SpecificSkill> SpecificSkills { get; set; } = new List<SpecificSkill>();
    }
}
