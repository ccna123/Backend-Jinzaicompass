using SystemBrightSpotBE.Dtos.Plan.UserPlan;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlanCondition
{
    public class LastConditionActivityDto
    {
        public long plan_condition_id { get; set; }
        public string plan_condition_name { get; set; } = string.Empty;
        public LastActivityDto last_activity { get; set; } = new LastActivityDto();
    }
}
