namespace SystemBrightSpotBE.Dtos.Tenant
{
    public class TenantDto
    {
        public long id { get; set; }
        public string name { get; set; } = String.Empty;
        public string first_name { get; set; } = String.Empty;
        public string last_name { get; set; } = String.Empty;
        public string email { get; set; } = String.Empty;
        public string phone { get; set; } = String.Empty;
        public string post_code { get; set; } = String.Empty;
        public string region { get; set; } = String.Empty;
        public string locality { get; set; } = String.Empty;
        public string street { get; set; } = String.Empty;
        public string? building_name { get; set; } = String.Empty;
        public string? comment { get; set; } = String.Empty;
        public DateOnly start_date { get; set; }
        public DateOnly end_date { get; set; }
        public long status { get; set; }
        public bool send_mail { get; set; } = false;
        public DateTime updated_at;
        public DateTime created_at;
    }
}
