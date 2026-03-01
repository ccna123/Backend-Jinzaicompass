using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.User
{
    public class CreateUserCertificationDto
    {
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "CertificationDateRequired")]
        public required DateTime certified_date { get; set; }
        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "CertificationIdRequired")]
        public required long certification_id { get; set; }
    }
}
