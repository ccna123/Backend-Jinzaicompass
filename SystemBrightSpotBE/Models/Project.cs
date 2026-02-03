using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Project : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string name { get; set; } = String.Empty;
        [Required]
        public string content { get; set; } = String.Empty;
        public string? description { get; set; }
        [Required]
        public DateOnly start_date { get; set; }
        public DateOnly? end_date { get; set; }
        [Required]
        [ForeignKey("Company")]
        public long company_id { get; set; }
        public Company? Company { get; set; }
        [ForeignKey("User")]
        public long? user_id { get; set; }
        public User? User { get; set; }
        public ICollection<ProjectExperienceJob>? ProjectExperienceJob { get; set; }
        public ICollection<ProjectExperienceField>? ProjectExperienceField { get; set; }
        public ICollection<ProjectExperienceArea>? ProjectExperienceArea { get; set; }
        public ICollection<ProjectSpecificSkill>? ProjectSpecificSkill { get; set; }
        public ICollection<ProjectParticipationPosition>? ProjectParticipationPosition { get; set; }
        public ICollection<ProjectParticipationProcess>? ProjectParticipationProcess { get; set; }
    }
}
