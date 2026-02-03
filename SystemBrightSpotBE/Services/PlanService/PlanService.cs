using AutoMapper;
using log4net;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Plan;
using SystemBrightSpotBE.Dtos.Plan.LogActivity;
using SystemBrightSpotBE.Dtos.Plan.PlanCondition;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;
using SystemBrightSpotBE.Dtos.User;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.PlanService
{
    public class PlanService : IPlanService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        private readonly IAuthService _authService;

        public PlanService(
            DataContext context,
            IMapper mapper,
            IAuthService authService
        ) {
            _log = LogManager.GetLogger(typeof(PlanService));
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<List<ListPlanDto>> GetAll(PlanParamDto request)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long departmentId = (long)_authService.GetAccountId("Department");
            long divisionId = (long)_authService.GetAccountId("Division");
            long groupId = (long)_authService.GetAccountId("Group");
            long roleId = (long)_authService.GetAccountId("Role");
            long tenantId = _authService.GetAccountId("Tenant");

            var query = _context.plans
                .Where(p => p.tenant_id == tenantId)
                .Include(p => p.UserPlan)
                .AsQueryable();

            if (roleId != (long)RoleEnum.MEMBER)
            {
                if (request.department_id > 0)
                {
                    query = query.Where(p => p.department_id == request.department_id);
                }
                    
                if (request.division_id > 0)
                {
                    query = query.Where(p => p.division_id == request.division_id);
                }

                if (request.status > 0)
                {
                    query = query.Where(p => p.status == request.status);
                }
            }

            if (roleId == (long)RoleEnum.MEMBER)
            {
                query = query.Where(p => p.UserPlan!.Any(m => m.user_id == userId && (request.status <= 0 || m.status == request.status)));
            }

            var result = await query
                .OrderByDescending(p => p.created_at)
                .Select(p => new ListPlanDto
                {
                    id = p.id,
                    name = p.name,
                    department_id = p.department_id,
                    division_id = p.division_id,
                    group_id = p.group_id,
                    status = p.status ?? 0,
                    created_at = p.created_at
                }).ToListAsync();

            return result;
        }

        public async Task CreatePlan(CreatePlanDto request, UserDto user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                long userId = (long)user.id;
                var departmentId = user.department_id ?? 0;
                var divisionId = user.division_id ?? 0;
                var groupId = user.group_id ?? 0;

                // Mapper plan
                var planCreate = _mapper.Map<Plan>(request);
                planCreate.user_id = userId;
                planCreate.department_id = departmentId;
                planCreate.division_id = divisionId != 0 ? divisionId : null;
                planCreate.group_id = groupId != 0 ? groupId : null;
                planCreate.created_at = DateTime.Now;
                planCreate.updated_at = DateTime.Now;

                await _context.plans.AddAsync(planCreate);
                await _context.SaveChangesAsync();
                // Save plan condition
                if (request.conditions != null && request.conditions.Any())
                {
                    var conditions = request.conditions.Select(planCd => new PlanCondition
                    {
                        name = planCd.name,
                        overview = planCd.overview,
                        est_time = planCd.est_time,
                        plan_id = planCreate.id,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    }).ToList();

                    await _context.plan_conditions.AddRangeAsync(conditions);
                    await _context.SaveChangesAsync();
                }
                // Commit async
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error create plan: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePlan(long id, UpdatePlanDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var plan = await _context.plans.FindAsync(id);
                if (plan == null)
                {
                    throw new Exception("Plan not found");
                }
                // Mapper plan
                _mapper.Map(request, plan);
                plan.updated_at = DateTime.Now;

                // Delete old plan condition
                var oldConditions = _context.plan_conditions.Where(pc => pc.plan_id == plan.id);
                _context.plan_conditions.RemoveRange(oldConditions);
                await _context.SaveChangesAsync();

                // Save new plan condition
                List<PlanCondition> newPlanConditions = new();
                if (request.conditions != null && request.conditions.Any())
                {
                    newPlanConditions = request.conditions.Select(planCd => new PlanCondition
                    {
                        name = planCd.name,
                        overview = planCd.overview,
                        est_time = planCd.est_time,
                        plan_id = plan.id,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    }).ToList();

                    await _context.plan_conditions.AddRangeAsync(newPlanConditions);
                    await _context.SaveChangesAsync();
                }

                // Check user plan exist
                var userPlans = await _context.user_plan
                    .Where(up => up.plan_id == plan.id)
                    .ToListAsync();

                if (userPlans.Any() && newPlanConditions.Any())
                {
                    var userPlanConditions = userPlans
                        .SelectMany(up => newPlanConditions.Select(pc => new UserPlanCondition
                        {
                            user_id = up.user_id,
                            user_plan_id = up.id,
                            plan_condition_id = pc.id
                        }))
                        .ToList();

                    await _context.user_plan_condition.AddRangeAsync(userPlanConditions);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error update plan {id}: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePlan(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var plan = await _context.plans
                    .Include(p => p.PlanCondition)
                    .FirstOrDefaultAsync(p => p.id == id);

                if (plan == null)
                {
                    throw new Exception("Plan not found");
                }

                // Delete plan condition
                if (plan.PlanCondition != null && plan.PlanCondition.Any())
                {
                    _context.plan_conditions.RemoveRange(plan.PlanCondition);
                }

                // Delete plan
                _context.plans.Remove(plan);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error deleting plan {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<PlanDto> FindById(long id)
        {
            var plan = await _context.plans
                    .FirstOrDefaultAsync(plan => plan.id == id);

            return _mapper.Map<PlanDto>(plan);
        }

        public async Task<UserPlanDto> FindByUserPlanId(long planId, long userId)
        {
            var result = await _context.user_plan
                .Where(up => up.plan_id == planId && up.user_id == userId)
                .Select(up => new UserPlanDto
                {
                    user_plan_id = up.id,
                    plan_id = up.plan_id,
                    user_id = up.user_id,
                    status = up.status
                })
                .FirstOrDefaultAsync();

            return result!;
        }

        public async Task<PlanDto> GeneralPlan(long id)
        {
            var result = await _context.plans
                .Where(p => p.id == id)
                .Select(p => new PlanDto
                {
                    id = p.id,
                    name = p.name,
                    description = p.description,
                    start_date = p.start_date,
                    complete_date = p.complete_date,
                    conditions = p.PlanCondition!.Select(c => new PlanConditionDto
                    {
                        name = c.name ?? "" ,
                        overview = c.overview ?? "",
                        est_time = c.est_time ?? 1
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return result!;
        }

        public async Task<DetailPlanDto> DetailPlan(long id)
        {
            var plan = await _context.plans
                .Include(p => p.PlanCondition)
                .Include(p => p.UserPlan)
                    .ThenInclude(up => up.User)
                .Include(p => p.UserPlan)
                    .ThenInclude(up => up.UserPlanCondition)
                        .ThenInclude(upc => upc.PlanCondition)
                .FirstOrDefaultAsync(p => p.id == id);

            if (plan != null)
            {
                long userId = (long)_authService.GetAccountId("Id");
                long roleId = (long)_authService.GetAccountId("Role");

                var userPlans = roleId == (long)RoleEnum.MEMBER ? plan.UserPlan!.Where(up => up.user_id == userId).ToList() : plan.UserPlan!.ToList();

                var detailPlan = new DetailPlanDto
                {
                    id = plan.id,
                    name = plan.name,
                    start_date= plan.start_date,
                    complete_date = plan.complete_date,
                    status = plan.status,
                    total_est_time = plan.PlanCondition!.Sum(pc => pc.est_time ?? 0),
                    total_user = plan.UserPlan!.Count(),
                    total_user_complete = plan.UserPlan!.Count(up => up.status == (long)UserPlanStatusEnum.COMPLETED),
                    total_user_in_progress = plan.UserPlan!.Count(up => up.status == (long)UserPlanStatusEnum.IN_PROGRESS),
                    total_user_pendding = plan.UserPlan!.Count(up => up.status == (long)UserPlanStatusEnum.PENDING_APPROVAL),
                    conditions = plan.PlanCondition!.Select(pc => new PlanConditionDto
                    {
                        name = pc.name ?? String.Empty,
                        overview = pc.overview ?? String.Empty,
                        est_time = pc.est_time ?? 1
                    }).ToList(),
                    users = userPlans.OrderByDescending(up => up.id).Select(up => new UserPlanDto
                    {
                        user_plan_id = up.id,
                        user_id = up.user_id,
                        user_fullname = $"{up.User!.last_name} {up.User!.first_name}",
                        plan_id = up.plan_id,
                        status = up.status,
                        total_condition_complete = up.UserPlanCondition!.Count(up => up.status == (long)UserPlanConditionStatusEnum.COMPLETED),
                        total_condition_in_complete = up.UserPlanCondition!.Count(up => up.status == (long)UserPlanConditionStatusEnum.IN_COMPLETE),
                        total_condition_pendding = up.UserPlanCondition!.Count(up => up.status == (long)UserPlanConditionStatusEnum.PENDING_APPROVAL),
                        conditions = up.UserPlanCondition!.Select(upc => new UserPlanConditionDto
                        {
                            user_plan_condition_id = upc.id,
                            user_id = upc.user_id,
                            plan_condition_id = upc.plan_condition_id,
                            plan_condition_name = upc.PlanCondition!.name ?? String.Empty,
                            plan_condition_overview = upc.PlanCondition!.overview ?? String.Empty,
                            plan_condition_est_time = upc.PlanCondition!.est_time,
                            status = upc.status
                        }).ToList()
                    }).ToList()
                };

                return detailPlan;
            } 
            else
            {
                throw new Exception("Plan not found");
            }
        }

        public async Task CreateAllocationByPlanId(long planId, long userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.user_plan.FirstOrDefaultAsync(up => up.plan_id == planId && up.user_id == userId);
                if (existing != null)
                {
                    return;
                }

                // Create new relation user plan
                var userPlan = new UserPlan
                {
                    user_id = userId,
                    plan_id = planId
                };

                _context.user_plan.Add(userPlan);
                await _context.SaveChangesAsync();

                // Get all condition by plan
                var planCondition = await _context.plan_conditions.Where(pc => pc.plan_id == planId).ToListAsync();
                var userPlanCondition = planCondition.Select(pc => new UserPlanCondition
                {
                    user_id = userPlan.user_id,
                    plan_condition_id = pc.id,
                    user_plan_id = userPlan.id
                }).ToList();

                _context.user_plan_condition.AddRange(userPlanCondition);

                // Update status plan
                var plan = await _context.plans.FindAsync(planId);
                if (plan != null)
                {
                    plan.status = (long)PlanStatusEnum.IN_PROGRESS;
                    _context.plans.Update(plan);
                }

                // Save change and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error add allocation by plan {planId}: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveAllocationByPlanId(long planId, long userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userPlan = await _context.user_plan.FirstOrDefaultAsync(up => up.plan_id == planId && up.user_id == userId);
                if (userPlan != null)
                {
                    var userPlanCondition = await _context.user_plan_condition
                       .Where(upc => upc.user_plan_id == userPlan.id)
                       .ToListAsync();

                    // Remove user plan condition
                    if (userPlanCondition.Any())
                    {
                        _context.user_plan_condition.RemoveRange(userPlanCondition);
                    }

                    // Remove user plan
                    _context.user_plan.Remove(userPlan);
                    await _context.SaveChangesAsync();

                    // Update status plan
                    var remainingUserPlans = await _context.user_plan.AnyAsync(up => up.plan_id == planId);
                    if (!remainingUserPlans)
                    {
                        var plan = await _context.plans.FindAsync(planId);
                        if (plan != null)
                        {
                            plan.status = (long)PlanStatusEnum.NO_START;
                            _context.plans.Update(plan);
                        }
                    } 
                    else
                    {
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
                            }
                        }
                    }

                    // Save change and commit transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error remove allocation by plan {planId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HasPermissionDeleteAllocation(long planId, long userId)
        {
            var userPlan = await FindByUserPlanId(planId, userId);

            if (userPlan != null) { 
                if (userPlan.status == (long)UserPlanStatusEnum.IN_PROGRESS)
                {
                    var allInCompleteCondition = await _context.user_plan_condition
                        .Where(upc => upc.user_plan_id == userPlan.user_plan_id)
                        .AllAsync(upc => upc.status == (long)UserPlanConditionStatusEnum.IN_COMPLETE);

                    if (allInCompleteCondition)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<LastUserPlanActivityDto> GetLastActivity(long planId, long userId)
        {
            var userPlan = await _context.user_plan
                .Where(up => up.plan_id == planId && up.user_id == userId)
                .Include(up => up.UserPlanActivity)
                .Include(up => up.UserPlanCondition!)
                    .ThenInclude(upc => upc.PlanCondition)
                .Include(up => up.UserPlanCondition!)
                    .ThenInclude(upc => upc.UserPlanConditionActivity)
                .FirstOrDefaultAsync();

            if (userPlan == null)
            {
                throw new Exception("User plan not found");
            }

            var lastPlanActivity = userPlan.UserPlanActivity?
                .OrderByDescending(upa => upa.updated_at)
                .Select(upa => new LastActivityDto
                {
                    user_id = upa.user_id,
                    status = upa.status,
                    updated_at = upa.updated_at
                })
                .FirstOrDefault();

            var lastConditionActivitiy = userPlan.UserPlanCondition?
                .Select(upc => new LastConditionActivityDto
                {
                    plan_condition_id = upc.plan_condition_id,
                    plan_condition_name = upc.PlanCondition!.name,
                    last_activity = upc.UserPlanConditionActivity!
                        .OrderByDescending(act => act.updated_at)
                        .Select(act => new LastActivityDto
                        {
                            user_id = act.user_id,
                            status = act.status,
                            updated_at = act.updated_at
                        })
                        .FirstOrDefault()
                }).ToList();

            return new LastUserPlanActivityDto
            {
                user_plan_id = userPlan.id,
                user_plan_activity = lastPlanActivity!,
                user_plan_condition_activity = lastConditionActivitiy!
            };
        }

        public async Task<List<LogActivityDto>> GetLogActivity(long planId, long userId)
        {
            var logs = new List<LogActivityDto>();

            var userPlan = await _context.user_plan
                .Where(up => up.plan_id == planId && up.user_id == userId)
                .Include(up => up.UserPlanActivity)
                    .ThenInclude(up => up.User)
                .Include(up => up.UserPlanCondition!)
                    .ThenInclude(upc => upc.PlanCondition)
                .Include(up => up.UserPlanCondition!)
                    .ThenInclude(upc => upc.UserPlanConditionActivity)
                        .ThenInclude(upca => upca.User)
                .FirstOrDefaultAsync();

            if (userPlan != null)
            {
                if (userPlan.UserPlanActivity != null)
                {
                    logs.AddRange(userPlan.UserPlanActivity.Select(act => new LogActivityDto
                    {
                        type = "plan",
                        condition_name = string.Empty,
                        status = act.status,
                        updated_at = act.updated_at,
                        user_id = act.user_id,
                        user_fullname = $"{act.User?.last_name} {act.User?.first_name}"
                    }));
                }

                if (userPlan.UserPlanCondition != null)
                {
                    foreach (var cond in userPlan.UserPlanCondition)
                    {
                        var condName = cond.PlanCondition?.name ?? String.Empty;

                        if (cond.UserPlanConditionActivity != null)
                        {
                            logs.AddRange(cond.UserPlanConditionActivity.Select(apct => new LogActivityDto
                            {
                                type = "condition",
                                condition_name = condName,
                                status = apct.status,
                                updated_at = apct.updated_at,
                                user_id = apct.user_id,
                                user_fullname = $"{apct.User?.last_name} {apct.User?.first_name}"
                            }));
                        }
                    }
                }
            }

            return logs.OrderBy(a => a.updated_at).ToList();
        }

        public async Task<PlanConditionActivityDto> GetDetailPlanActivityByUser(long planId, long userId)
        {
            var plan = await _context.plans
                .Include(p => p.PlanCondition)
                .FirstOrDefaultAsync(p => p.id == planId);

            var userPlan = await _context.user_plan
                .Include(up => up.UserPlanActivity!)
                    .ThenInclude(upa => upa.User)
                .Include(up => up.UserPlanCondition!)
                    .ThenInclude(upc => upc.UserPlanConditionActivity)
                        .ThenInclude(upca => upca.User)
                    
                .FirstOrDefaultAsync(up => up.plan_id == planId && up.user_id == userId);

            var detailPlanActivity = new PlanConditionActivityDto();

            if (plan != null && userPlan != null)
            {
                detailPlanActivity.plan_id = plan.id;
                detailPlanActivity.name = plan.name;
                detailPlanActivity.description = plan.description;
                detailPlanActivity.start_date = plan.start_date;
                detailPlanActivity.complete_date = plan.complete_date;
                detailPlanActivity.status = userPlan.status;
                detailPlanActivity.total_condition_complete = userPlan.UserPlanCondition!.Count(up => up.status == (long)UserPlanConditionStatusEnum.COMPLETED);
                detailPlanActivity.total_condition_in_complete = userPlan.UserPlanCondition!.Count(up => up.status == (long)UserPlanConditionStatusEnum.IN_COMPLETE);
                detailPlanActivity.total_condition_pendding = userPlan.UserPlanCondition!.Count(up => up.status == (long)UserPlanConditionStatusEnum.PENDING_APPROVAL);
                detailPlanActivity.activities = userPlan.UserPlanActivity!
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
                    .ToList();
                detailPlanActivity.conditions = plan.PlanCondition!.Select(pc =>
                {
                    var userPlanCondition = userPlan!.UserPlanCondition!.FirstOrDefault(uc => uc.plan_condition_id == pc.id);
                    var activities = new List<UserPlanConditionActivityDto>();
                    var userPlanConditionDto = new UserPlanConditionDto();

                    if (userPlanCondition != null)
                    {
                        activities = userPlanCondition!.UserPlanConditionActivity!
                           .OrderBy(a => a.updated_at)
                           .Select(a => new UserPlanConditionActivityDto
                           {
                               id = a.id,
                               comment = a.comment ?? String.Empty,
                               status = a.status,
                               file_name = a.file_name ?? String.Empty,
                               file_url = a.file_url ?? String.Empty,
                               user_id = a.user_id,
                               user_fullname = $"{a.User!.last_name} {a.User!.first_name}",
                               user_plan_condition_id = a.user_plan_condition_id,
                               revoke_flag = a.revoke_flag,
                               updated_at = a.updated_at,
                               revoked_at = a.revoked_at,
                           }).ToList();

                        userPlanConditionDto.user_plan_condition_id = userPlanCondition.id;
                        userPlanConditionDto.user_id = userPlanCondition.user_id;
                        userPlanConditionDto.plan_condition_id = pc.id;
                        userPlanConditionDto.plan_condition_name = pc.name ?? String.Empty;
                        userPlanConditionDto.plan_condition_overview = pc.overview ?? String.Empty;
                        userPlanConditionDto.plan_condition_est_time = pc.est_time;
                        userPlanConditionDto.status = userPlanCondition.status;
                        userPlanConditionDto.activities = activities;
                    }

                    return userPlanConditionDto;
                }).ToList();
            }

            return detailPlanActivity;
        }
    }
}
