using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Skill
{
    public class SpecificSkillDto : SkillDto
    {
        public string type { get; set; } = SkillTypeEnum.specific_skill.ToString();
        public bool delete_flag { get; set; } = true;
        public required long experience_area_id { get; set; }
    }
}
