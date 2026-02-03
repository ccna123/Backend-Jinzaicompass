using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserCompanyAward : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("CompanyAward")]
        public long company_award_id { get; set; }
        public CompanyAward? CompanyAward { get; set; }
        [Required]
        public DateTime awarded_date { get; set; }
    }
}
