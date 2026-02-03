using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Tenant
{
    public class UpdateTenantDto
    {
        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "NameRequired")]
        [MaxLength(255, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "NameMaxLength")]
        public string name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "EmailRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "EmailMaxLength")]
        [RegularExpression(@"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,4}$",ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "EmailRegx")]
        public string email { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "FirstNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "FirstNameMaxLength")]
        public string first_name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "LastNameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "LastNameMaxLength")]
        public string last_name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "PhoneRequired")]
        [MaxLength(16, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "PhoneMaxLength")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "PhoneRegx")]
        public string phone { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "PostCodeRequired")]
        [MaxLength(7, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "PostCodeMaxLength")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "PostCodeRegx")]
        public string post_code { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "RegionRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "RegionMaxLength")]
        public string region { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "LocalityRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "LocalityMaxLength")]
        public string locality { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "StreetRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "StreetMaxLength")]
        public string street { get; set; } = String.Empty;

        [MaxLength(64, ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "BuidingNameMaxLength")]
        public string building_name { get; set; } = String.Empty;

        public string? comment { get; set; } = String.Empty;
    }
}
