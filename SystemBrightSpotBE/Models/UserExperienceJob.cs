using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserExperienceJob : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("ExperienceJob")]
        public long experience_job_id { get; set; }
        public ExperienceJob? ExperienceJob { get; set; }
    }
}
