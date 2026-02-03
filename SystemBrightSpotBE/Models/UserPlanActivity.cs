using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserPlanActivity : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string? comment { get; set; }
        [Comment(@"
            1: Accepted
            2: Submitted
            3: Rejected
            4: Revoked"
        )]
        public long? status { get; set; }
        public bool? revoke_flag { get; set; } = false;
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("UserPlan")]
        public long user_plan_id { get; set; }
        public UserPlan? UserPlan { get; set; }
        public DateTime? revoked_at { get; set; }
    }
}
