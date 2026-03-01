using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Auth
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "NewPasswordRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "NewPasswordMaxLength")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,64}$",
            ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "PasswordRegx")]
        public required string new_password { get; set; }
        [Required(ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "ConfirmPasswordRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "ConfirmPasswordMaxLength")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,64}$",
            ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "PasswordRegx")]
        [Compare("new_password", ErrorMessageResourceType = typeof(AuthResource), ErrorMessageResourceName = "ConfirmPasswordCompare")]
        public required string confirm_password { get; set; }
    }
}