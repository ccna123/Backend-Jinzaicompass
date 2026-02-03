namespace SystemBrightSpotBE.Dtos.Report
{
    public class ReportDivisionDto
    {
        public long report_id { get; set; }
        public long division_id { get; set; }
        public string division_name { get; set; } = string.Empty;
    }
}
