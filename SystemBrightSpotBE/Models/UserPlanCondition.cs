using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserPlanCondition
    {
        [Key]
        public long id { get; set; }
        [Comment(@"
            1: 完了 (completed)
            2: 未完了 (incomplete)
            3: 承認待ち (pendding approval)")]
        public long? status { get; set; } = 2;
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("PlanCondition")]
        public long plan_condition_id { get; set; }
        public PlanCondition? PlanCondition { get; set; }
        [Required]
        [ForeignKey("UserPlan")]
        public long user_plan_id { get; set; }
        public UserPlan? UserPlan { get; set; }
        public ICollection<UserPlanConditionActivity>? UserPlanConditionActivity { get; set; }
    }
}
