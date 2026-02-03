
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ProjectExperienceField : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("Project")]
        public long project_id { get; set; }
        public Project? Project { get; set; }
        [Required]
        [ForeignKey("ExperienceField")]
        public long experience_field_id { get; set; }
        public ExperienceField? ExperienceField { get; set; }
    }
}
