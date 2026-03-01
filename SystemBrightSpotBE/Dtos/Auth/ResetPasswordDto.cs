using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Auth
{
    public class ResetPasswordDto
    {

        [Required(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "EmailRequired")]
        [EmailAddress(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "EmailRegx")]
        [MaxLength(64, ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "EmailMaxLength")]
        public required string email { get; set; }
    }
}