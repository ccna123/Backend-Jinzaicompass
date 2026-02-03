namespace SystemBrightSpotBE.Dtos.User
{
    public class UserListDto
    {
        public long id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string code { get; set; } = String.Empty;
        public string? department_name { get; set; }
        public string? division_name { get; set; }
        public string? group_name { get; set; }
        public string? position_name { get; set; }
        public string? employment_type_name { get; set; }
        public string? employment_status_name { get; set; }
        public DateOnly? date_joining_company { get; set; }
        public bool? active { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
