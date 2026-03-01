using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "EmailRequired")]
        [EmailOrAdministrator(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "EmailRegx")]
        [MaxLength(64, ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "EmailMaxLength")]
        public required string email { get; set; }
        [Required(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "PasswordRequired")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,64}$",
        //    ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "PasswordRegx")]
        [MaxLength(64, ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "PasswordMaxLength")]
        public required string password { get; set; }
    }
}
