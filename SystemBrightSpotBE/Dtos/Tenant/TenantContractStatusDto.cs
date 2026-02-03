namespace SystemBrightSpotBE.Dtos.Tenant
{
    public class TenantContractStatusDto
    {
        public long tenant_id { get; set; }
        public string tenant_email { get; set; } = string.Empty;
        public int remain_days { get; set; }
    }
}
