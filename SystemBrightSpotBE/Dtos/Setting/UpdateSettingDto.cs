using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Setting
{
    public class UpdateSettingDto
    {
        [Required(ErrorMessageResourceType = typeof(SettingResource), ErrorMessageResourceName = "NameRequired")]
        [MaxLength(36, ErrorMessageResourceType = typeof(SettingResource), ErrorMessageResourceName = "NameMaxLength")]
        public string name { get; set; } = String.Empty;

        [AllowedContentTypeAttribute(new[] { "image/jpeg", "image/png", "image/svg+xml" }, MaxWidth = 1000, MaxHeight = 1000, MaxSize = 5)]
        public IFormFile? file { get; set; }
    }
}
