using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Skill
{
    public class ExperienceFieldDto : SkillDto
    {
        public string type { get; set; } = SkillTypeEnum.experience_field.ToString();
        public bool delete_flag { get; set; } = true;
        public required long experience_job_id { get; set; }
        public List<ExperienceAreaDto> children { get; set; } = new List<ExperienceAreaDto>();
    }
}
