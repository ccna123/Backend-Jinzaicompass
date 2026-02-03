using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserExperienceField : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("ExperienceField")]
        public long experience_field_id { get; set; }
        public ExperienceField? ExperienceField { get; set; }
    }
}
