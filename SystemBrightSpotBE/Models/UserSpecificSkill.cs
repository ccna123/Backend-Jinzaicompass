using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserSpecificSkill : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("SpecificSkill")]
        public long specific_skill_id { get; set; }
        public SpecificSkill? SpecificSkill { get; set; }
    }
}
