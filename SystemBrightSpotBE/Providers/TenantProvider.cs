namespace SystemBrightSpotBE.Providers
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? GetTenantId()
        {
            var tenant = _httpContextAccessor.HttpContext?.User?.FindFirst("Tenant")?.Value;

            if (long.TryParse(tenant, out long tenantId))
            {
                return tenantId;
            }

            return null;
        }
    }
}
