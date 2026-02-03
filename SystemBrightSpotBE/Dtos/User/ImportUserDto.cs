using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Attributes;

namespace SystemBrightSpotBE.Dtos.User
{
    public class ImportUserDto
    {
        [Required]
        [AllowedContentTypeAttribute(new[] { "text/csv", "application/vnd.ms-excel" }, MaxSize = 100)]
        public required IFormFile file { get; set; }
    }
}
