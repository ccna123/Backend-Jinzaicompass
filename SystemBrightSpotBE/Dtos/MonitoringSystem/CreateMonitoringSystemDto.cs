using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.MonitoringSystem
{
    public class CreateMonitoringSystemDto
    {
        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "FirstNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "FirstNameMaxLength")]
        public string first_name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "LastNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "LastNameMaxLength")]
        public string last_name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "EmailRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "EmailMaxLength")]
        [RegularExpression(@"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,4}$", ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "EmailRegx")]
        public string email { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PhoneRequired")]
        [MaxLength(16, ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PhoneMaxLength")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(MonitoringSystemResource), ErrorMessageResourceName = "PhoneRegx")]
        public string phone { get; set; } = String.Empty;
    }
}
