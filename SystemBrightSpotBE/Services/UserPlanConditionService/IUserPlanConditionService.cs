using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;
using SystemBrightSpotBE.Enums;

namespace SystemBrightSpotBE.Services.UserPlanConditionService
{
    public interface IUserPlanConditionService
    {
        Task<UserPlanConditionDto> GetActivity(long id);
        Task CreateActivity(long id, CreateConditionActivityDto request);
        Task UpdateActivity(long activityId, UpdateConditionActivityDto request);
        Task<UserPlanConditionDto> FindById(long id);
        Task<UserPlanConditionActivityDto> FindByActivityId(long id);
        Task<bool> HasPermisstion(long id, ActivityStatusEnum type);
        Task<bool> HasPermisstionByActivity(long activityId, ActivityStatusEnum type);
    }
}
