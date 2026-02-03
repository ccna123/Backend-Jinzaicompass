namespace SystemBrightSpotBE.Dtos.User
{
    public class ListUserParamDto
    {
        public int page { get; set; } = 1;
        public int size { get; set; } = 20;
        public string column { get; set; } = "updated_at";
        public string order { get; set; } = "desc";
        public string? full_name { get; set; }
        public int? gender_id { get; set; }
        public int? certification_id { get; set; }
        public int? employment_type_id { get; set; }
    }
}