
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ProjectParticipationProcess : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("Project")]
        public long project_id { get; set; }
        public Project? Project { get; set; }
        [Required]
        [ForeignKey("ParticipationProcess")]
        public long participation_process_id { get; set; }
        public ParticipationProcess? ParticipationProcess { get; set; }
    }
}
