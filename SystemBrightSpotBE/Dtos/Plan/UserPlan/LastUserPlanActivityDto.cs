using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlan
{
    public class LastUserPlanActivityDto
    {
        public long user_plan_id { get; set; }
        public LastActivityDto user_plan_activity { get; set; } = new LastActivityDto();
        public List<LastConditionActivityDto> user_plan_condition_activity { get; set; } = new List<LastConditionActivityDto>();
    }
}
