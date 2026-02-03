using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserPlan
    {
        [Key]
        public long id { get; set; }
        [Comment(@"
            1: 完了 (completed: Status after the completion request is approved)
            2: 進行中 (in progress: Status after the plan is allocated)
            3: 承認待ち (pending approval: Status after the completion request is submitted)")]
        public long status { get; set; } = 2;
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("Plan")]
        public long plan_id { get; set; }
        public Plan? Plan { get; set; }
        public ICollection<UserPlanCondition>? UserPlanCondition { get; set; }
        public ICollection<UserPlanActivity>? UserPlanActivity { get; set; }
    }
}
