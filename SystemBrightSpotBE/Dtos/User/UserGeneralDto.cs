using SystemBrightSpotBE.Dtos.UserStatusHistory;

namespace SystemBrightSpotBE.Dtos.User
{
    public class UserGeneralDto
    {
        public long id { get; set; }
        public string avatar { get; set; } = String.Empty;
        public string first_name { get; set; } = String.Empty;
        public string last_name { get; set; } = String.Empty;
        public string code { get; set; } = String.Empty;
        public string? department_name { get; set; }
        public string? employment_type_name { get; set; }
        public long? gender_id { get; set; }
        public string gender_name { get; set; } = String.Empty;
        public DateOnly? date_of_birth { get; set; }
        public string? phone { get; set; } = String.Empty;
        public string? address { get; set; } = String.Empty;
        public string? nearest_station { get; set; } = String.Empty;
        public DateTime? deleted_at { get; set; }
    }
}