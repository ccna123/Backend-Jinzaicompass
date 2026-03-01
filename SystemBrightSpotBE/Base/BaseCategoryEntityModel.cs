using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Base
{
    public class BaseCategoryEntityModel : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Maxlength 255 characters")]
        public string name { get; set; } = String.Empty;
        [Comment(@"
            0: 許可されていません
            1: 許可されています")]
        public bool? delete_flag { get; set; } = true;
    }
}
