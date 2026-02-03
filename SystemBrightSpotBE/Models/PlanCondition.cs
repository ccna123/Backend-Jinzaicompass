using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class PlanCondition : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string? name { get; set; }
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string? overview { get; set; }
        public int? est_time { get; set; }
        [Required]
        [ForeignKey("Plan")]
        public long plan_id { get; set; }
        public Plan? Plan { get; set; }
    }
}
