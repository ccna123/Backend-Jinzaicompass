using AutoMapper;
using log4net;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.UserPlanService
{
    public class UserPlanService : IUserPlanService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        private readonly IAuthService _authService;

        public UserPlanService(
            DataContext context,
            IMapper mapper,
            IAuthService authService
        )
        {
            _log = LogManager.GetLogger(typeof(UserPlanService));
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<List<UserPlanActivityDto>> GetActivity(long id)
        {
            var activities = await _context.user_plan_activity
                .Where(a => a.user_plan_id == id)
                .OrderBy(a => a.updated_at)
                .Select(a => new UserPlanActivityDto
                {
                    id = a.id,
                    comment = a.comment ?? string.Empty,
                    status = a.status,
                    revoke_flag = a.revoke_flag,
                    user_id = a.user_id,
                    user_fullname = $"{a.User!.last_name} {a.User!.first_name}",
                    user_plan_id = a.user_plan_id,
                    updated_at = a.updated_at,
                    revoked_at = a.revoked_at
                })
                .ToListAsync();

            return activities!;
        }

        public async Task CreateActivity(long id, CreateActivityDto request)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");
            var userPlan = await _context.user_plan
               .Where(up => up.id == id)
               .FirstOrDefaultAsync();

            if (userPlan != null)
            {
                var activity = new UserPlanActivity
                {
                    comment = request.comment,
                    status = (long)request.type,
                    revoke_flag = false,
                    revoked_at = null,
                    user_id = userId,
                    user_plan_id = userPlan.id,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Create user plan actitity
                    _context.user_plan_activity.Add(activity);

                    if (roleId == (long)RoleEnum.MEMBER)
                    {
                        if (request.type == ActivityStatusEnum.SUBMITTED)
                        {
                            userPlan.status = (long)UserPlanStatusEnum.PENDING_APPROVAL;
                        }
                    } 
                    else
                    {
                        if (request.type == ActivityStatusEnum.ACCEPTED || request.type == ActivityStatusEnum.SUBMITTED)
                        {
                            userPlan.status = (long)UserPlanStatusEnum.COMPLETED;
                        } 
                        else if (request.type == ActivityStatusEnum.REJECTED)
                        {
                            userPlan.status = (long)UserPlanStatusEnum.IN_PROGRESS;
                        }
                    }
                    // Update status user plan
                    _context.user_plan.Update(userPlan);
                    await _context.SaveChangesAsync();

                    // Check all user complete plan
                    var allUserCompleted = await _context.user_plan
                       .Where(up => up.plan_id == userPlan.plan_id)
                       .AllAsync(up => up.status == (long)UserPlanStatusEnum.COMPLETED);

                    if (allUserCompleted)
                    {
                        var plan = await _context.plans.FindAsync(userPlan.plan_id);
                        if (plan != null)
                        {
                            plan.status = (long)PlanStatusEnum.COMPLETED;
                            _context.plans.Update(plan);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                   
                    throw new Exception("Create user plan activity error: " + ex.Message, ex);
                }
            }
        }

        public async Task UpdateActivity(long activityId, UpdateActivityDto request)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");

            var activity = await _context.user_plan_activity
                .Include(a => a.UserPlan)
                .FirstOrDefaultAsync(a => a.id == activityId);

            if (activity != null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Update user plan activity
                    activity.comment = request.comment;
                    activity.status = (long)request.type;
                    activity.revoke_flag = request.type == ActivityStatusEnum.REVOKED;
                    activity.revoked_at = request.type == ActivityStatusEnum.REVOKED ? DateTime.Now : null;
                    activity.updated_at = DateTime.Now;

                    _context.user_plan_activity.Update(activity);
                    // Update user plan status
                    var userPlan = activity.UserPlan;
                    if (userPlan != null)
                    {
                        if (roleId == (long)RoleEnum.MEMBER)
                        {
                            if (request.type == ActivityStatusEnum.REVOKED)
                            {
                                userPlan.status = (long)UserPlanStatusEnum.IN_PROGRESS;
                            }
                        }
                        else
                        {
                            if (request.type == ActivityStatusEnum.REVOKED)
                            {
                                userPlan.status = (long)UserPlanStatusEnum.PENDING_APPROVAL;
                            }
                        }

                        _context.user_plan.Update(userPlan);
                        await _context.SaveChangesAsync();

                        // Check all user complete plan
                        var allUserCompleted = await _context.user_plan
                           .Where(up => up.plan_id == userPlan.plan_id)
                           .AllAsync(up => up.status == (long)UserPlanStatusEnum.COMPLETED);

                        var plan = await _context.plans.FindAsync(userPlan.plan_id);

                        if (allUserCompleted)
                        {
                            if (plan != null)
                            {
                                plan.status = (long)PlanStatusEnum.COMPLETED;
                                _context.plans.Update(plan);
                            }
                        }
                        else
                        {
                            if (plan != null)
                            {
                                plan.status = (long)PlanStatusEnum.IN_PROGRESS;
                                _context.plans.Update(plan);
                            }
                        }

                        // Update status plan
                        await _context.SaveChangesAsync();
                    }
                   
                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Update user plan activity error: " + ex.Message, ex);
                }
            }
        }

        public async Task<UserPlanDto> FindById(long id)
        {
            var userPlan = await _context.user_plan
               .Where(up => up.id == id)
               .Select(up => new UserPlanDto
               {
                   user_plan_id = up.id,
                   plan_id = up.plan_id,
                   user_id = up.user_id,
                   status = up.status
               })
               .FirstOrDefaultAsync();

            return userPlan!;
        }

        public async Task<UserPlanActivityDto> FindByActivityId(long id)
        {
            var activity = await _context.user_plan_activity
                    .FirstOrDefaultAsync(upa => upa.id == id);

            return _mapper.Map<UserPlanActivityDto>(activity);
        }

        public async Task<bool> HasPermisstionByUserPlan(long id, ActivityStatusEnum type)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");

            var userPlan = await FindById(id);
            if (userPlan != null) {
                // Check all user plan condition statuses have been approved or not
                var allCompleteCondition = await _context.user_plan_condition
                    .Where(upc => upc.user_plan_id == id)
                    .AllAsync(upc => upc.status == (long)UserPlanConditionStatusEnum.COMPLETED);

                if (allCompleteCondition)
                {
                    if (roleId == (long)RoleEnum.MEMBER)
                    {
                        if (userPlan.user_id == userId && userPlan.status == (long)UserPlanStatusEnum.IN_PROGRESS && type == ActivityStatusEnum.SUBMITTED)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (userPlan.status == (long)UserPlanStatusEnum.PENDING_APPROVAL && (type == ActivityStatusEnum.ACCEPTED || type == ActivityStatusEnum.REJECTED))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public async Task<bool> HasPermisstionByUserPlanActivity(long activityId, ActivityStatusEnum type)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");

            var userPlanActivity = await _context.user_plan_activity
                    .Include(upa => upa.UserPlan)
                    .FirstOrDefaultAsync(upa => upa.id == activityId);

            if (userPlanActivity != null)
            {
                if (roleId == (long)RoleEnum.MEMBER)
                {
                    if (userPlanActivity.user_id == userId && userPlanActivity.UserPlan!.status == (long)UserPlanStatusEnum.PENDING_APPROVAL)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}
