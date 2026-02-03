namespace SystemBrightSpotBE.Dtos.Organization
{
    public class CreateOrganizationDto : BaseOrganizationDto
    {
        public long? department_id { get; set; }
        public long? division_id { get; set; }
    }
}
