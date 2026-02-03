using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Plan.UserPlan
{
    public class CreateUserPlanDto
    {
        [Required]
        public long user_id { get; set; }  
    }
}
