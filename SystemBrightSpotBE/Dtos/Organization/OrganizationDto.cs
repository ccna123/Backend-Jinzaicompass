namespace SystemBrightSpotBE.Dtos.Organization
{
    public class OrganizationDto
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public bool delete_flag { get; set; } = true;
        public List<OrganizationDto> children { get; set; } = new();
    }
}
