namespace SystemBrightSpotBE.Dtos.Report
{
    public class ReportUserDto
    {
        public long report_id { get; set; }
        public long user_id { get; set; }
        public string user_fullname { get; set; } = string.Empty;
    }
}
