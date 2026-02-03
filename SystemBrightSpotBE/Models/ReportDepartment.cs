using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ReportDepartment : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [ForeignKey("Report")]
        public long report_id { get; set; }
        public Report? Report { get; set; }

        [Required]
        [ForeignKey("Department")]
        public long department_id { get; set; }
        public Department? Department { get; set; }
    }
}
