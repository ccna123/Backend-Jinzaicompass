using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Services.UserPlanService
{
    public interface IUserPlanService
    {
        Task<List<UserPlanActivityDto>> GetActivity(long id);
        Task CreateActivity(long id, CreateActivityDto request);
        Task UpdateActivity(long activityId, UpdateActivityDto request);
        Task<UserPlanDto> FindById(long id);
        Task<UserPlanActivityDto> FindByActivityId(long id);
        Task<bool> HasPermisstionByUserPlan(long id, ActivityStatusEnum type);
        Task<bool> HasPermisstionByUserPlanActivity(long activityId, ActivityStatusEnum type);
    }
}
