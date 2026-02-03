using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Contact
{
    public class CreateContactDto
    {
        [Required(ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "NameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "NameMaxLength")]
        public string name { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "EmailRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "EmailMaxLength")]
        [RegularExpression(@"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,4}$", ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "EmailRegx")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "PhoneRequired")]
        [MaxLength(16, ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "PhoneMaxLength")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "PhoneRegx")]
        public string phone { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "TitleRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "TitleMaxLength")]
        public string title { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(ContactResource), ErrorMessageResourceName = "ContentRequired")]
        public string content { get; set; } = string.Empty;
    }
}
