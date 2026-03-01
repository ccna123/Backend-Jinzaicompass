using SystemBrightSpotBE.Dtos.Category;
using SystemBrightSpotBE.Dtos.Skill;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Services.SkillService
{
    public interface ISkillService
    {
        Task<List<ExperienceJobDto>> GetTree();
        Task<List<SkillDto>> GetExperienceJob();
        Task<List<SkillDto>> GetExperienceFieldByJob(string ids);
        Task<List<SkillDto>> GetExperienceAreaByField(string ids);
        Task<List<SkillDto>> GetSpecificSkillByArea(string ids);
        Task<CategoryDto?> FindById(long id, SkillTypeEnum? type);
        Task<bool> CheckNameExistAsync(BaseSkillDto request, bool update = false, long id = 0);
        Task Create(CreateSkillDto request);
        Task Update(long id, UpdateSkillDto request);
        Task Delete(long id, SkillTypeEnum? type);
        Task<bool> HasCheck(long id, SkillTypeEnum? type);
    }
}
