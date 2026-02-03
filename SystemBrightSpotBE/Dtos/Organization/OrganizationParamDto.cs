using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Organization
{
    public class OrganizationParamDto
    {
        [Required]
        public required long id { get; set; }
    }
}
