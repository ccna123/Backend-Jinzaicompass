using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.User
{
    public class CreateUserCompanyAwardDto
    {
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "CompanyAwardDateRequired")]
        public required DateTime awarded_date { get; set; }
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "CompanyAwardIdRequired")]
        public required long company_award_id { get; set; }
    }
}
