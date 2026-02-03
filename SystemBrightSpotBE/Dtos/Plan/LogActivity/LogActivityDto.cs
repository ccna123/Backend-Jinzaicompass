namespace SystemBrightSpotBE.Dtos.Plan.LogActivity
{
    public class LogActivityDto
    {
        public string type { get; set; } = String.Empty;
        public string condition_name {  get; set; } = String.Empty;
        public long? status { get; set; }
        public long user_id { get; set; }
        public string user_fullname { get; set; } = String.Empty;
        public DateTime? updated_at { get; set; }
    }
}
