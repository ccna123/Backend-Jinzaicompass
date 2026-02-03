namespace SystemBrightSpotBE.Dtos.Plan.UserPlanCondition
{
    public class UserPlanConditionActivityDto
    {
        public long id { get; set; }
        public string comment { get; set; } = string.Empty;
        public long? status { get; set; } = 2;
        public string file_name {  get; set; } = string.Empty;
        public string file_url { get; set; } = string.Empty;
        public bool? revoke_flag { get; set; } = false;
        public long user_id { get; set; }
        public string user_fullname { get; set; } = string.Empty;
        public long user_plan_condition_id { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? revoked_at { get; set; }
    }
}
