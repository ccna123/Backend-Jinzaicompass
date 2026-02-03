using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Plan : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string name { get; set; } = String.Empty;
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        [Required]
        public string description { get; set; } = String.Empty;
        [Required]
        public DateOnly start_date { get; set; }
        [Required]
        public DateOnly complete_date { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("Department")]
        public long department_id { get; set; }
        public Department? Department { get; set; }
        [ForeignKey("Division")]
        public long? division_id { get; set; }
        public Division? Division { get; set; }
        [ForeignKey("Group")]
        public long? group_id { get; set; }
        public Group? Group { get; set; }
        [Comment(@"
            1: 完了 (completed: all member plan status completed)
            2: 進行中 (in progress: the plan has ≥1 members)
            3: 未着手 (not started: the plan has no members)
        ")]
        public long? status { get; set; } = 3;
        public DateTime? deleted_at { get; set; }
        public ICollection<PlanCondition>? PlanCondition { get; set; }
        public ICollection<UserPlan>? UserPlan { get; set; }
    }
}
