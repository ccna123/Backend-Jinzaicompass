using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Skill
{
    public class ExperienceAreaDto : SkillDto
    {
        public string type { get; set; } = SkillTypeEnum.experience_area.ToString();
        public bool delete_flag { get; set; } = true;
        public required long experience_field_id { get; set; }
        public List<SpecificSkillDto> children { get; set; } = new List<SpecificSkillDto>();
    }
}
