namespace SystemBrightSpotBE.Dtos.Plan.UserPlan
{
    public class UserPlanActivityDto
    {
        public long id { get; set; }
        public string comment { get; set; } = string.Empty;
        public long? status { get; set; } = 2;
        public bool? revoke_flag { get; set; } = false;
        public long user_id {  get; set; }
        public string user_fullname { get; set; } = string.Empty;
        public long user_plan_id { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? revoked_at { get; set; }
    }
}
