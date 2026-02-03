using SystemBrightSpotBE.Dtos.Plan.UserPlan;

namespace SystemBrightSpotBE.Dtos.Plan
{
    public class DetailPlanDto : PlanDto
    {
        public int total_est_time { get; set; }
        public int total_user {  get; set; }
        public int total_user_complete { get; set; }
        public int total_user_in_progress { get; set; }
        public int total_user_pendding { get; set; }
        public List<UserPlanDto> users { get; set; } = new();
    }
}
