using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Setting : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string name { get; set; } = String.Empty;
        public string? file_name { get; set; } = String.Empty;
        public string? file_url { get; set; } = String.Empty;
        public string? file_url_thumb { get; set; } = String.Empty;
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
    }
}
