using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.MonitoringSystem
{
    public class MonitoringSystemChangePasswordDto
    {
        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "CurrentPasswordRequired")]
        public required string current_password { get; set; }

        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "NewPasswordRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PasswordMaxLength")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,64}$", ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PasswordRegx")]
        public required string new_password { get; set; }

        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "ConfirmPasswordRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PasswordMaxLength")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,64}$", ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PasswordRegx")]
        [Compare("new_password", ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "ConfirmPasswordCompare")]
        public required string confirm_password { get; set; }
    }
}