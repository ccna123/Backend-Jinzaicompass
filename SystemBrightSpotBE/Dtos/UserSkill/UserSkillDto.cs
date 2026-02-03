namespace SystemBrightSpotBE.Dtos.UserSkill
{
    public class UserSkillDto
    {
        public long user_id { get; set; }
        public List<UserExperienceJobDto> experience_job { get; set; } = new List<UserExperienceJobDto>();
        public List<UserExperienceFieldDto> experience_field { get; set; } = new List<UserExperienceFieldDto>();
        public List<UserExperienceAreaDto> experience_area { get; set; } = new List<UserExperienceAreaDto>();
        public List<UserSpecificSkillDto> specific_skill { get; set; } = new List<UserSpecificSkillDto>();
    }
}
