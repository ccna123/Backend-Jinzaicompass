using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.UserStatusHistory
{
    public class StatusHistoryDto
    {
        [Required]
        public int type { get; set; }
        [Required]
        public DateOnly date { get; set; }
    }
}
