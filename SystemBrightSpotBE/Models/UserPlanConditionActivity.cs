using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserPlanConditionActivity : BaseEntity
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
        public string? file_name { get; set; } = String.Empty;
        public string? file_url { get; set; } = String.Empty;
        public bool? revoke_flag { get; set; } = false;
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("UserPlanCondition")]
        public long user_plan_condition_id { get; set; }
        public UserPlanCondition? UserPlanCondition { get; set; }
        public DateTime? revoked_at { get; set; }
    }
}
