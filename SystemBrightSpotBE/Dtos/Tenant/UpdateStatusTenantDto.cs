using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Tenant
{
    public class UpdateStatusTenantDto
    {
        [Required]
        [EnumDataType(typeof(TenantActionEnum), ErrorMessage = "Action is invalid")]
        public long action { get; set; }
    }
}
