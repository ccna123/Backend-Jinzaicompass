namespace SystemBrightSpotBE.Dtos.Report
{
    public class ReportGroupDto
    {
        public long report_id { get; set; }
        public long group_id { get; set; }
        public string group_name { get; set; } = string.Empty;
    }
}
