using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.User;
using SystemBrightSpotBE.Dtos.UserCertification;
using SystemBrightSpotBE.Dtos.UserCompanyAward;
using SystemBrightSpotBE.Dtos.UserManager;
using SystemBrightSpotBE.Dtos.UserProject;
using SystemBrightSpotBE.Dtos.UserSkill;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Services.UserService
{
    public interface IUserService
    {
        Task<PagedResponse<List<UserListDto>>> GetAll(ListUserParamDto request);
        Task<List<long>> GetManagedUsersId(long? id = null);
        Task Create(AddUserDto request);
        Task<UserDto> FindById(long id);
        Task<UserGeneralDto?> FindByIdGeneral(long id);
        Task<long?> FindRoleByUserId(long id);
        Task<string?> FindPasswordByUserId(long id);
        Task<bool> CheckExistAsync(string field, string value, bool update = false, long id = 0);
        Task Update(long id, UpdateUserDto request, bool removeAvatar = false);
        Task UpdateGeneral(long id, UpdateUserGeneralDto request, bool removeAvatar = false);
        Task Delete(long id);
        Task<BaseResponse> Import(IFormFile file);
        Task<List<UserReportDto>> GetReportByTargetId(long id);
        Task<UserSkillDto> GetSkillByTargetId(long id);
        Task UpdateSkillByTargetId(long id, UpdateUserSkillDto request);
        Task<bool> CheckExistSkillAsync(string ids, SkillTypeEnum type);
        Task<bool> CheckExistParticipationAsync(string ids, ParticipationTypeEnum type);
        Task<List<UserCertificationDto>> GetCertificationByTargetId(long id);
        Task CreateCertificationByTargetId(long id, CreateUserCertificationDto request);
        Task DeleteCertificationByTargetId(long certificationId);
        Task<List<UserCompanyAwardDto>> GetCompanyAwardByTargetId(long id);
        Task CreateCompanyAwardByTargetId(long id, CreateUserCompanyAwardDto request);
        Task DeleteCompanyAwardByTargetId(long awardId);
        Task<List<UserProjectDto>> GetProjectByTargetId(long id);
        Task CreateProjectByTargetId(long id, CreateUserProjectDto request);
        Task UpdateProjectByTargetId(long projectId, UpdateUserProjectDto request);
        Task DeleteProjectByTargetId(long projectId);
        Task<UserProjectDto> FindByProjectId(long id);
        Task<SkillSheetDto> GetSkillSheetByTargetId(long id);
        Task<List<UserMemberDto>> GetMember(UserMemberParamDto request);
        Task<List<UserMemberDto>> GetTargetUsers();
        Task<List<UserMemberDto>> GetUserByDepartment(long? departmentId);
    }
}