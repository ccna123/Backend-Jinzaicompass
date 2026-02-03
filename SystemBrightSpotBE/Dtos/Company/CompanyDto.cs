namespace SystemBrightSpotBE.Dtos.Company
{
    public class CompanyDto
    {
        public long id { get; set; }
        public string name { get; set; } = String.Empty;
        public string? address { get; set; }
        public string? phone { get; set; }
        public DateTime updated_at { get; set; }
    }
}
