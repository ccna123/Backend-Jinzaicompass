using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Models
{
    public class Tenant : BaseEntity
    {
        [Key]
        public long id { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string name { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string first_name { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string last_name { get; set; } = String.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "MaxLength 255 characters")]
        public string email { get; set; } = String.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "MaxLength 20 characters")]
        public string phone { get; set; } = String.Empty;

        [StringLength(7, ErrorMessage = "MaxLength 7 characters")]
        public string post_code { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string region { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string locality { get; set; } = String.Empty;

        [Required]
        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string street { get; set; } = String.Empty;

        [StringLength(64, ErrorMessage = "MaxLength 64 characters")]
        public string? building_name { get; set; } = String.Empty;

        public string? comment { get; set; } = String.Empty;

        [Required]
        public DateOnly start_date { get; set; }
        [Required]
        public DateOnly end_date { get; set; }
        [Comment(@"
            1: InReview
            2: Scheduled
            3: Actived
            4: Suspended
            5: Expired
        ")]
        public long status { get; set; } = 1;

        public bool send_mail { get; set; } = false;
        public DateTime? send_at { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}
