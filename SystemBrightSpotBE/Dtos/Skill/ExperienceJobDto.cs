using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Skill
{
    public class ExperienceJobDto : SkillDto
    {
        public string type { get; set; } = SkillTypeEnum.experience_job.ToString();
        public bool delete_flag { get; set; } = true;
        public List<ExperienceFieldDto> children { get; set; } = new List<ExperienceFieldDto>();
    }
}
