namespace SystemBrightSpotBE.Dtos.Report
{
    public class ReportPDFDto
    {
        public string title { get; set; } = String.Empty;
        public string content { get; set; } = String.Empty;
        public DateOnly date { get; set; }  
        public string report_type_name { get; set; } = String.Empty;
        public string target_all { get; set; } = String.Empty;
        public string user_fullname { get; set; } = String.Empty;
    }
}
