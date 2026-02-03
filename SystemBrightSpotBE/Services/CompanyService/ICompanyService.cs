using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.Company;

namespace SystemBrightSpotBE.Services.CompanyService
{
    public interface ICompanyService
    {
        Task<List<CompanyDto>> GetAll();
        Task<PagedResponse<List<CompanyDto>>> GetPaginate(CompanyParamDto request);
        Task Create(CreateCompanyDto request);
        Task Update(long id, UpdateCompanyDto request);
        Task Delete(long id);
        Task<CompanyDto> FindById(long id);
        Task<bool> CheckNameExist(string value, bool update = false, long id = 0);
    }
}
