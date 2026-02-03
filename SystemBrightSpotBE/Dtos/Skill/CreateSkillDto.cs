namespace SystemBrightSpotBE.Dtos.Skill
{
    public class CreateSkillDto : BaseSkillDto
    {
        public long? experience_job_id { get; set; }
        public long? experience_field_id { get; set; }
        public long? experience_area_id { get; set; }
    }
}
