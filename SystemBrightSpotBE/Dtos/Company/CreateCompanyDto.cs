using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Company
{
    public class CreateCompanyDto
    {
        [Required(ErrorMessageResourceType = typeof(CompanyResource), ErrorMessageResourceName = "NameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(CompanyResource), ErrorMessageResourceName = "NameMaxLength")]
        public string name { get; set; } = String.Empty;

        [MaxLength(16, ErrorMessageResourceType = typeof(CompanyResource), ErrorMessageResourceName = "PhoneMaxLength")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(CompanyResource), ErrorMessageResourceName = "PhoneRegx")]
        public string phone { get; set; } = String.Empty;

        [MaxLength(64, ErrorMessageResourceType = typeof(CompanyResource), ErrorMessageResourceName = "AddressMaxLength")]
        public string address { get; set; } = String.Empty;
    }
}
