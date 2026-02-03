using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Notification : BaseEntity
    {
        [Key]
        public long id { get; set; }
        public string content { get; set; } = string.Empty;
        [Comment(@"
            ０：未読
            １：既読")]
        public bool is_read { get; set; } = false;

        [Required]
        [ForeignKey("Report")]
        public long report_id { get; set; }
        public Report? Report { get; set; }

        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
    }
}
