using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Models
{
    [Index(nameof(email), IsUnique = true)]
    public class MonitoringSystem : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string first_name { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string last_name { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string email { get; set; } = String.Empty;

        [Required]
        [StringLength(16, ErrorMessage = "MaxLength 16 characters")]
        public string phone { get; set; } = String.Empty;
    }
}
