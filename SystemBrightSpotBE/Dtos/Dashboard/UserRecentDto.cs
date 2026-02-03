namespace SystemBrightSpotBE.Dtos.Dashboard
{
    public class UserRecentDto
    {
        public string full_name { get; set; } = String.Empty;
        public string gender_name { get; set; } = String.Empty;
        public string department_name { get; set; } = String.Empty;
        public DateOnly date_joining_company { get; set; }
    }
}
