using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ExperienceField : BaseCategoryEntityModel
    {
        [Required]
        [ForeignKey("ExperienceJob")]
        public long experience_job_id { get; set; }
        public ExperienceJob? ExperienceJob { get; set; }

        public ICollection<ExperienceArea> ExperienceAreas { get; set; } = new List<ExperienceArea>();
    }
}
