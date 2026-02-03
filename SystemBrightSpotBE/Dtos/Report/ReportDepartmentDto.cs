namespace SystemBrightSpotBE.Dtos.Report
{
    public class ReportDepartmentDto
    {
        public long report_id { get; set; }
        public long department_id { get; set; }
        public string department_name { get; set; } = string.Empty;
    }
}
