using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Organization
{
    public class DepartmentDto : OrganizationDto
    {
        public DepartmentDto()
        {
            type = OrganizationTypeEnum.department.ToString();
        }
    }
}
