using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SystemBrightSpotBE.Base;

namespace SystemBrightSpotBE.Models
{
    public class ProjectExperienceJob : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("Project")]
        public long project_id { get; set; }
        public Project? Project { get; set; }
        [Required]
        [ForeignKey("ExperienceJob")]
        public long experience_job_id { get; set; }
        public ExperienceJob? ExperienceJob { get; set; }
    }
}
