using SystemBrightSpotBE.Dtos.Plan;
using SystemBrightSpotBE.Dtos.Plan.LogActivity;
using SystemBrightSpotBE.Dtos.Plan.PlanCondition;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Dtos.User;

namespace SystemBrightSpotBE.Services.PlanService
{
    public interface IPlanService
    {
        Task<List<ListPlanDto>> GetAll(PlanParamDto request);
        Task CreatePlan(CreatePlanDto request, UserDto user);
        Task UpdatePlan(long id, UpdatePlanDto request);
        Task DeletePlan(long id);
        Task<PlanDto> GeneralPlan(long id);
        Task<DetailPlanDto> DetailPlan(long id);
        Task<PlanDto> FindById(long id);
        Task<UserPlanDto> FindByUserPlanId(long planId, long userId);
        Task CreateAllocationByPlanId(long planId, long userId);
        Task RemoveAllocationByPlanId(long planId, long userId);
        Task<bool> HasPermissionDeleteAllocation(long planId, long userId);
        Task<LastUserPlanActivityDto> GetLastActivity(long planId, long userId);
        Task<List<LogActivityDto>> GetLogActivity(long planId, long userId);
        Task<PlanConditionActivityDto> GetDetailPlanActivityByUser(long planId, long userId);
    }
}
