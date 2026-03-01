using SystemBrightSpotBE.Dtos.UserStatusHistory;

namespace SystemBrightSpotBE.Dtos.User
{
    public class UserDto
    {
        public long id { get; set; }
        public string avatar { get; set; } = String.Empty;
        public string first_name { get; set; } = String.Empty;
        public string last_name { get; set; } = String.Empty;
        public string first_name_kana { get; set; } = String.Empty;
        public string last_name_kana { get; set; } = String.Empty;
        public string email { get; set; } = String.Empty;
        public string code { get; set; } = String.Empty;
        public long? gender_id { get; set; }
        public DateOnly? date_of_birth { get; set; }
        public string? phone { get; set; } = String.Empty;
        public string? address { get; set; } = String.Empty;
        public string? nearest_station { get; set; } = String.Empty;
        public long? role_id { get; set; } = 5;
        public long? department_id { get; set; }
        public long? division_id { get; set; }
        public long? group_id { get; set; }
        public long? position_id { get; set; }
        public long? employment_type_id { get; set; }
        public long? employment_status_id { get; set; }
        public bool? active { get; set; } = false;
        public string password { get; set; } = String.Empty;
        public bool? temp_password_used { get; set; } = true;
        public DateTime? temp_password_expired_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? gender_name { get; set; }
        public string? role_name { get; set; }
        public string? department_name { get; set; }
        public string? division_name { get; set; }
        public string? group_name { get; set; }
        public string? position_name { get; set; }
        public string? employment_type_name { get; set; }
        public string? employment_status_name { get; set; }
        public ICollection<StatusHistoryDto>? status_history { get; set; }
        public DateOnly? date_joining_company { get; set; }
    }
}
