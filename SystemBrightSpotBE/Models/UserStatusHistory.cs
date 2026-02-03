using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class UserStatusHistory : BaseEntity
    {
        [Key]
        public long id { get; set; }
        [Required]
        [Comment(@"
            1: 入社
            2: 休職
            3: 復帰
            4: 退職")]
        public int type { get; set; }
        [Required]
        public DateOnly date { get; set; }
        [Required]
        [ForeignKey("User")]
        public long user_id { get; set; }
        public User? User { get; set; }
    }
}
