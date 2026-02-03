using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlan
{
    public class UserPlanDto
    {
        public long user_plan_id { get; set; }
        public long plan_id { get; set; } 
        public long user_id { get; set; }
        public string user_fullname { get; set; } = String.Empty;
        public long status { get; set; }
        public int total_condition_complete { get; set; } = 0;
        public int total_condition_in_complete { get; set; } = 0;
        public int total_condition_pendding { get; set; } = 0;
        public List<UserPlanConditionDto> conditions { get; set; } = new();
    }
}
