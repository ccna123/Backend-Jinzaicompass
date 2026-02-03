using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Dtos.Organization;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Services.OrganizationService
{
    public interface IOrganizationService
    {
        Task<List<DepartmentDto>> GetTree();
        Task<DepartmentDto?> GetTreeDepartment(long? departmentId);
        Task<List<OrganizationDto>> GetDepartment();
        Task<List<OrganizationDto>> GetDivisionByDepartment(long id);
        Task<List<OrganizationDto>> GetGroupByDivision(long id);
        Task<List<OrganizationDto>> GetGroupByDepartment(long id);
        Task<CategoryDto?> FindById(long id, OrganizationTypeEnum? type);
        Task<bool> CheckNameExistAsync(BaseOrganizationDto request, bool update = false, long id = 0);
        Task Create(CreateOrganizationDto request);
        Task Update(long id, UpdateOrganizationDto request);
        Task Delete(long id, OrganizationTypeEnum? type);
        Task<bool> HasCheckUser(long id, OrganizationTypeEnum? type);
    }
}
