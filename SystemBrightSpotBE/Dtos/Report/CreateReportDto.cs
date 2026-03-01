using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Report
{
    public class CreateReportDto
    {
        [Required(ErrorMessageResourceType = typeof(ReportResource), ErrorMessageResourceName = "TitleRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(ReportResource), ErrorMessageResourceName = "TitleMaxLength")]
        public string title { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(ReportResource), ErrorMessageResourceName = "ContentRequired")]
        public string content { get; set; } = string.Empty;
        public DateOnly date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required(ErrorMessageResourceType = typeof(ReportResource), ErrorMessageResourceName = "ReportTypeRequired")]
        public long report_type_id { get; set; }

        public bool? is_public { get; set; } = true;
        public string? department_ids { get; set; }
        public string? division_ids { get; set; }
        public string? group_ids { get; set; }
        public string? user_ids { get; set; }
    }
}
