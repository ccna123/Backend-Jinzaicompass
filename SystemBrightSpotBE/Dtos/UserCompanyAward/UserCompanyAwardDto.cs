namespace SystemBrightSpotBE.Dtos.UserCompanyAward
{
    public class UserCompanyAwardDto
    {
        public long id { get; set; }
        public DateTime awarded_date { get; set; }
        public long company_award_id { get; set; }
        public string company_award_name { get; set; } = String.Empty;
        public long user_id { get; set; }
    }
}
