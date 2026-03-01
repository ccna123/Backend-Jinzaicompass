using SystemBrightSpotBE.Dtos.UserSkill;

namespace SystemBrightSpotBE.Dtos.UserProject
{
    public class UserProjectDto
    {
        public long id { get; set; }
        public string name { get; set; } = String.Empty;
        public string content { get; set; } = String.Empty;
        public string? description { get; set; }
        public DateOnly start_date { get; set; }
        public DateOnly? end_date { get; set; }
        public long company_id { get; set; }
        public string company_name { get; set; } = String.Empty;
        public List<ProjectExperienceJobDto> experience_job { get; set; } = new List<ProjectExperienceJobDto>();
        public List<ProjectExperienceFieldDto> experience_field { get; set; } = new List<ProjectExperienceFieldDto>();
        public List<ProjectExperienceAreaDto> experience_area { get; set; } = new List<ProjectExperienceAreaDto>();
        public List<ProjectSpecificSkillDto> specific_skill { get; set; } = new List<ProjectSpecificSkillDto>();
        public List<ProjectParticipationPositionDto> participation_position { get; set; } = new List<ProjectParticipationPositionDto> { };
        public List<ProjectParticipationProcessDto> participation_process { get; set; } = new List<ProjectParticipationProcessDto> { };
    }
}
