using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;

namespace SystemBrightSpotBE.Dtos.Plan.PlanCondition
{
    public class PlanConditionActivityDto
    {
        public long plan_id { get; set; }
        public string name { get; set; } = String.Empty;
        public string description { get; set; } = String.Empty;
        public DateOnly start_date { get; set; }
        public DateOnly complete_date { get; set; }
        public long? status { get; set; }
        public int total_condition_complete { get; set; } = 0;
        public int total_condition_in_complete { get; set; } = 0;
        public int total_condition_pendding { get; set; } = 0;
        public List<UserPlanActivityDto> activities { get; set; } = new List<UserPlanActivityDto> { };
        public List<UserPlanConditionDto> conditions { get; set; } = new List<UserPlanConditionDto> { };
    }
}
