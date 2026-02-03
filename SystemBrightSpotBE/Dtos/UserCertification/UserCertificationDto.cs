namespace SystemBrightSpotBE.Dtos.UserCertification
{
    public class UserCertificationDto
    {
        public long id { get; set; }
        public DateTime certified_date { get; set; }
        public long certification_id { get; set; }
        public string certification_name { get; set; } = String.Empty;
        public long user_id { get; set; }
    }
}
