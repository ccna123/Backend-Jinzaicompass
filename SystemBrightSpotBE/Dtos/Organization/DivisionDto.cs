using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Organization
{
    public class DivisionDto : OrganizationDto
    {
        public required long department_id { get; set; }
        public DivisionDto()
        {
            type = OrganizationTypeEnum.division.ToString();
        }
    }
}
