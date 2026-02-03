using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserCertification : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("Certification")]
        public long certification_id { get; set; }
        public Certification? Certification { get; set; }
        [Required]
        public DateTime certified_date { get; set; }
    }
}
