namespace SystemBrightSpotBE.Dtos.Company
{
    public class CompanyParamDto
    {
        public int page { get; set; } = 1;
        public int size { get; set; } = 20;
        public string column { get; set; } = "updated_at";
        public string order { get; set; } = "desc";
    }
}
