using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ReportGroup : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [ForeignKey("Report")]
        public long report_id { get; set; }
        public Report? Report { get; set; }

        [Required]
        [ForeignKey("Group")]
        public long group_id { get; set; }
        public Group? Group { get; set; }
    }
}
