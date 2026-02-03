using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.Tenant;

namespace SystemBrightSpotBE.Services.TenantService
{
    public interface ITenantService
    {
        Task<PagedResponse<List<TenantDto>>> GetAll(TenantParamDto request);
        Task<TenantDto> Create(CreateTenantDto request);
        Task Update(long id, UpdateTenantDto request);
        Task UpdateExpired(long id, DateOnly endDate);
        Task UpdateStatus(long id, long status);
        Task<bool> CheckEmailExist(string value, bool update = false, long id = 0);
        Task<CheckExpiredDto> CheckChangeExpried(long id, DateOnly endDate);
        Task<bool> CheckChangeStatus(long id, long action);
        Task<TenantDto> FindById(long id);
   
        Task<List<TenantContractStatusDto>> GetTenantContractStatus();
    }
}
