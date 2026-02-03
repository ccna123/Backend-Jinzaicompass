namespace SystemBrightSpotBE.Dtos.Plan.UserPlanCondition
{
    public class UserPlanConditionDto
    {
        public long user_plan_condition_id { get; set; }
        public long user_id { get; set; }
        public long plan_condition_id { get; set; }
        public string plan_condition_name { get; set; } = String.Empty;
        public string plan_condition_overview { get; set; } = String.Empty;
        public int? plan_condition_est_time { get; set; }
        public long? status { get; set; } = 2;
        public List<UserPlanConditionActivityDto> activities { get; set; } = new List<UserPlanConditionActivityDto>();
    }
}
