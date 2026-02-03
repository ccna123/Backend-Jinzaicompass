namespace SystemBrightSpotBE.Dtos.UserManager
{
    public class UserMemberDto
    {
        public long id { get; set; }
        public string first_name { get; set; } = String.Empty;
        public string last_name { get; set; } = String.Empty;
        public long department_id { get; set; }
        public long? division_id { get; set; }
        public long? group_id { get; set; }
    }
}
