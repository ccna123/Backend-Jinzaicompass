using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Report : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string title { get; set; } = String.Empty;

        [Required]
        public string content { get; set; } = String.Empty;

        [Required]
        public DateOnly date { get; set; }

        [Required]
        [ForeignKey("ReportType")]
        public long report_type_id { get; set; }
        public ReportType? ReportType { get; set; }

        [Comment(@"
            1：公開
            0：非公開")]
        public bool? is_public { get; set; } = true;

        [Required]
        public long user_id { get; set; }
        [ForeignKey(nameof(user_id))]
        public User? User { get; set; }

        public DateTime? deleted_at { get; set; }

        public ICollection<ReportDepartment>? ReportDepartment { get; set; }
        public ICollection<ReportDivision>? ReportDivision { get; set; }
        public ICollection<ReportGroup>? ReportGroup { get; set; }
        public ICollection<ReportUser>? ReportUser { get; set; }

    }
}
