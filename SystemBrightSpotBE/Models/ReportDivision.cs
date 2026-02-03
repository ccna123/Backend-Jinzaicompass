using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ReportDivision : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [ForeignKey("Report")]
        public long report_id { get; set; }
        public Report? Report { get; set; }

        [Required]
        [ForeignKey("Division")]
        public long division_id { get; set; }
        public Division? Division { get; set; }
    }
}
