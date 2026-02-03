using AutoMapper;
using log4net;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.S3Service;

namespace SystemBrightSpotBE.Services.UserPlanConditionService
{
    public class UserPlanConditionService : IUserPlanConditionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        private readonly IAuthService _authService;
        private readonly IS3Service _s3Service;

        public UserPlanConditionService(
            DataContext context,
            IMapper mapper,
            IAuthService authService,
            IS3Service s3Service
        )
        {
            _log = LogManager.GetLogger(typeof(UserPlanConditionService));
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _s3Service = s3Service;
        }

        public async Task<UserPlanConditionDto> GetActivity(long id)
        {
            var userPlanCondition = await _context.user_plan_condition
                .Include(upc => upc.PlanCondition)
                .FirstOrDefaultAsync(upc => upc.id == id);

            var result = new UserPlanConditionDto();

            if (userPlanCondition != null)
            {
                var activities = await _context.user_plan_condition_activity
                    .Where(a => a.user_plan_condition_id == id)
                    .OrderBy(a => a.updated_at)
                    .Select(a => new UserPlanConditionActivityDto
                    {
                        id = a.id,
                        comment = a.comment ?? String.Empty,
                        status = a.status,
                        file_name = a.file_name ?? String.Empty,
                        file_url = a.file_url ?? String.Empty,
                        updated_at = a.updated_at,
                        user_id = a.user_id,
                        user_fullname = $"{a.User!.last_name} {a.User!.first_name}",
                        user_plan_condition_id = a.user_plan_condition_id
                    })
                    .ToListAsync();

                result = new UserPlanConditionDto
                {
                    user_plan_condition_id = userPlanCondition.id,
                    user_id = userPlanCondition.user_id,
                    status = userPlanCondition.status,
                    plan_condition_id = userPlanCondition.plan_condition_id,
                    plan_condition_name = userPlanCondition!.PlanCondition!.name ?? String.Empty,
                    plan_condition_overview = userPlanCondition!.PlanCondition!.overview ?? String.Empty,
                    plan_condition_est_time = userPlanCondition!.PlanCondition!.est_time,
                    activities = activities
                };
            }

            return result;
        }

        public async Task CreateActivity(long id, CreateConditionActivityDto request)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");
            var userPlanCondition = await _context.user_plan_condition
               .Where(upc => upc.id == id)
               .FirstOrDefaultAsync();

            if (userPlanCondition != null)
            {
                string fileName = String.Empty;
                string fileUrl = String.Empty;

                if (request.file != null && request.file.Length > 0)
                {     
                    fileName = request.file.FileName;
                    var resultUpload = await _s3Service.UploadFileAsync(request.file, folder: "plan", width: null);
                    if (resultUpload != null)
                    {
                        fileUrl = resultUpload.ToString();
                    }
                }

                var activity = new UserPlanConditionActivity
                {
                    comment = request.comment,
                    status = (long)request.type,
                    file_name = fileName,
                    file_url = fileUrl,
                    revoke_flag = false,
                    revoked_at = null,
                    user_id = userId,
                    user_plan_condition_id = userPlanCondition.id,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Create user plan condition actitity
                    _context.user_plan_condition_activity.Add(activity);

                    if (roleId == (long)RoleEnum.MEMBER)
                    {
                        if (request.type == ActivityStatusEnum.SUBMITTED)
                        {
                            userPlanCondition.status = (long)UserPlanConditionStatusEnum.PENDING_APPROVAL;
                        }
                    }
                    else
                    {
                        if (request.type == ActivityStatusEnum.ACCEPTED || request.type == ActivityStatusEnum.SUBMITTED)
                        {
                            userPlanCondition.status = (long)UserPlanConditionStatusEnum.COMPLETED;
                        }
                        else if (request.type == ActivityStatusEnum.REJECTED)
                        {
                            userPlanCondition.status = (long)UserPlanConditionStatusEnum.IN_COMPLETE;
                        }
                    }
                    // Update status user plan condition
                    _context.user_plan_condition.Update(userPlanCondition);

                    // Save change
                    await _context.SaveChangesAsync();
                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    throw new Exception("Create user plan condition activity error: " + ex.Message, ex);
                }
            }
        }

        public async Task UpdateActivity(long activityId, UpdateConditionActivityDto request)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");

            var activity = await _context.user_plan_condition_activity
                .Include(a => a.UserPlanCondition)
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

                    _context.user_plan_condition_activity.Update(activity);
                    // Remove file if revoked request
                    if (roleId == (long)RoleEnum.MEMBER && request.type == ActivityStatusEnum.REVOKED &&  !String.IsNullOrEmpty(activity.file_url))
                    {
                        await _s3Service.DeleteFileAsync(activity.file_url);
                    }
                    // Update user plan status
                    var userPlanCondition = activity.UserPlanCondition;
                    if (userPlanCondition != null)
                    {
                        if (roleId == (long)RoleEnum.MEMBER)
                        {
                            if (request.type == ActivityStatusEnum.REVOKED)
                            {
                                userPlanCondition.status = (long)UserPlanConditionStatusEnum.IN_COMPLETE;
                            }
                        }
                        else
                        {
                            if (request.type == ActivityStatusEnum.REVOKED)
                            {
                                userPlanCondition.status = (long)UserPlanConditionStatusEnum.PENDING_APPROVAL;
                            }
                        }

                        _context.user_plan_condition.Update(userPlanCondition);
                    }
                    // Save change
                    await _context.SaveChangesAsync();
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

        public async Task<UserPlanConditionDto> FindById(long id)
        {
            var userPlanCondition = await _context.user_plan_condition
              .Where(upc => upc.id == id)
              .Select(upc => new UserPlanConditionDto
              {
                  user_plan_condition_id = upc.id,
                  user_id = upc.user_id,
                  plan_condition_id = upc.plan_condition_id,
                  status = upc.status
              })
              .FirstOrDefaultAsync();

            return userPlanCondition!;
        }

        public async Task<UserPlanConditionActivityDto> FindByActivityId(long id)
        {
            var activity = await _context.user_plan_condition_activity
                    .FirstOrDefaultAsync(upa => upa.id == id);

            return _mapper.Map<UserPlanConditionActivityDto>(activity);
        }

        public async Task<bool> HasPermisstion(long id, ActivityStatusEnum type)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");

            var userPlanCondition = await FindById(id);
            if (userPlanCondition != null)
            {
                if (roleId == (long)RoleEnum.MEMBER)
                {
                    if (userPlanCondition.user_id == userId && userPlanCondition.status == (long)UserPlanConditionStatusEnum.IN_COMPLETE && type == ActivityStatusEnum.SUBMITTED)
                    {
                        return true;
                    }
                }
                else
                {
                    if (userPlanCondition.status == (long)UserPlanStatusEnum.PENDING_APPROVAL && (type == ActivityStatusEnum.ACCEPTED || type == ActivityStatusEnum.REJECTED))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<bool> HasPermisstionByActivity(long activityId, ActivityStatusEnum type)
        {
            long userId = (long)_authService.GetAccountId("Id");
            long roleId = (long)_authService.GetAccountId("Role");

            var userPlanConditionActivity = await _context.user_plan_condition_activity
                    .Include(upa => upa.UserPlanCondition)
                    .FirstOrDefaultAsync(upa => upa.id == activityId);

            if (userPlanConditionActivity != null)
            {
                if (roleId == (long)RoleEnum.MEMBER)
                {
                    if (userPlanConditionActivity.user_id == userId && userPlanConditionActivity.UserPlanCondition!.status == (long)UserPlanConditionStatusEnum.PENDING_APPROVAL)
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
