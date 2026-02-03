using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Skill
{
    public class SkillParamDto
    {
        [Required]
        public required string ids { get; set; }
    }
}
