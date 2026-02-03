using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Base
{
    public class BaseCategoryModel
    {
        [Key]
        public long id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Maxlength 255 characters")]
        public string name { get; set; } = String.Empty;
    }
}
