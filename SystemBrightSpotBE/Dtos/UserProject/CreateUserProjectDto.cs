using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.UserSkill
{
    public class CreateUserProjectDto
    {
        [Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "NameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "NameMaxLength")]
        public string name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "ContentRequired")]
        [MaxLength(255, ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "ContentMaxLength")]
        public string content { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "StartDateRequired")]
        public DateOnly start_date { get; set; }

        //[Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "EndDateRequired")]
        public DateOnly? end_date { get; set; }

        [Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "CompanyIdRequired")]
        public long company_id { get; set; }

        [Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "ParticipationPositionRequired")]
        public string participation_position { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "ParticipationProcessRequired")]
        public string participation_process { get; set; } = String.Empty;
        public string experience_job { get; set; } = String.Empty;
        public string experience_field { get; set; } = String.Empty;
        public string experience_area { get; set; } = String.Empty;
        public string specific_skill { get; set; } = String.Empty;

        [MaxLength(255, ErrorMessageResourceType = typeof(ProjectResource), ErrorMessageResourceName = "DiscriptionMaxLength")]
        public string description { get; set; } = String.Empty;
    }
}
