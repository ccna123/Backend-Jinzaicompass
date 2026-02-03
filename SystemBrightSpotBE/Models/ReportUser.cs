using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ReportUser : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [ForeignKey("Report")]
        public long report_id { get; set; }
        public Report? Report { get; set; }

        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
    }
}
