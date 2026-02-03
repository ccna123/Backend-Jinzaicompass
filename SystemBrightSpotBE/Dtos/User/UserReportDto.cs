namespace SystemBrightSpotBE.Dtos.User
{
    public class UserReportDto
    {
        public long id { get; set; }
        public string title { get; set; } = String.Empty;
        public DateOnly date { get; set; }
        public long report_type_id { get; set; }
        public string report_type_name { get; set; } = String.Empty;
        public string full_name { get; set; } = String.Empty;
        public DateTime created_at { get; set; }
    }
}
