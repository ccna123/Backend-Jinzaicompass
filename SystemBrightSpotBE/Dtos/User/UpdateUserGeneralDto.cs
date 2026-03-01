using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.User
{
    public class UpdateUserGeneralDto
    {
        [AllowedContentTypeAttribute(new[] { "image/jpeg", "image/png", "image/svg+xml" })]
        public IFormFile? avatar { get; set; }

        [Required(ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "GenderRequired")]
        [Range(1, 3, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "GenderRegx")]
        public long? gender_id { get; set; }

        public DateOnly? date_of_birth { get; set; }

        [MaxLength(16, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "PhoneMaxLength")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "PhoneRegx")]
        public string? phone { get; set; } = String.Empty;

        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "AddressMaxLength")]
        public string? address { get; set; } = String.Empty;

        [MaxLength(64, ErrorMessageResourceType = typeof(UserResource), ErrorMessageResourceName = "NearestStationMaxLength")]
        public string? nearest_station { get; set; } = String.Empty;
    }
}
