using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Models
{
    public class Company : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string name { get; set; } = String.Empty;
        [StringLength(16, ErrorMessage = "MaxLength 16 characters")]
        public string? phone { get; set; }
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string? address { get; set; }
    }
}
