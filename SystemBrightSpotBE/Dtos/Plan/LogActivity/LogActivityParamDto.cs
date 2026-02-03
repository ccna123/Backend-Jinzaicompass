using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Plan.LogActivity
{
    public class LogActivityParamDto
    {
        [Required]
        public long user_id { get; set; }
    }
}
