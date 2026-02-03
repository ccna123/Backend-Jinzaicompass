
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class ProjectParticipationPosition : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [ForeignKey("Project")]
        public long project_id { get; set; }
        public Project? Project { get; set; }
        [Required]
        [ForeignKey("ParticipationPosition")]
        public long participation_position_id { get; set; }
        public ParticipationPosition? ParticipationPosition { get; set; }
    }
}
