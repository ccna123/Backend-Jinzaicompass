using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Dtos.Organization
{
    public class GroupDto : OrganizationDto
    {
        public long? division_id { get; set; }
        public long department_id { get; set; }
        public GroupDto()
        {
            type = OrganizationTypeEnum.group.ToString();
        }
    }
}
