using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Plan;
using SystemBrightSpotBE.Dtos.Plan.LogActivity;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.PlanService;
using SystemBrightSpotBE.Services.UserService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : BaseController
    {
        private readonly ILog _log;
        private readonly IPlanService _planService;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        
        public PlanController(
            IPlanService planService,
            IUserService userService,
            IAuthService authService
        )
        {
            _log = LogManager.GetLogger(typeof(PlanController));
            _planService = planService;
            _userService = userService;
            _authService = authService;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult<BaseResponse>> GetAll([FromQuery] PlanParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var data = await _planService.GetAll(request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("create")]
        [AuthorizePermission("PLAN02")]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreatePlanDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            long userId = (long)_authService.GetAccountId("Id");

            var user = await _userService.FindById(userId);

            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            if (user.department_id == 0 || user.department_id == null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, Message: ApiResource.DepartmentNotFound);
            }

            try
            {
                await _planService.CreatePlan(request, user);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/general")]
        [AuthorizePermission("PLAN03")]
        public async Task<ActionResult<BaseResponse>> General(long id)
        {
            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }

            try
            {
                var planGeneral = await _planService.GeneralPlan(plan.id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: planGeneral);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }


        [Authorize]
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<BaseResponse>> Detail(long id)
        {
            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }

            try
            {
                var detail = await _planService.DetailPlan(plan.id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: detail);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update")]
        [AuthorizePermission("PLAN03")]
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] UpdatePlanDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            } 
            else
            {
                if (plan.status != (int) PlanStatusEnum.NO_START)
                {
                    return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
                }
            }

            try
            {
                await _planService.UpdatePlan(plan.id, request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/delete")]
        [AuthorizePermission("PLAN03")]
        public async Task<ActionResult<BaseResponse>> Delete(long id)
        {
            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }
            else
            {
                if (plan.status != (int)PlanStatusEnum.NO_START)
                {
                    return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
                }
            }

            try
            {
                await _planService.DeletePlan(plan.id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("{id}/allocation")]
        [AuthorizePermission("PLAN04")]
        public async Task<ActionResult<BaseResponse>> CreateAllocation(long id, [FromBody] CreateUserPlanDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            } 
            else
            {
                if (plan.status == (long)PlanStatusEnum.COMPLETED)
                {
                    return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
                }
            }


            if (request.user_id > 0)
            {
                var user = await _userService.FindById(request.user_id);
                if (user is null || user.deleted_at != null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
                }
                else
                {
                    if (user.role_id != (long)RoleEnum.MEMBER)
                    {
                        return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
                    }
                }
            }

            try
            {
                await _planService.CreateAllocationByPlanId(plan.id, request.user_id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpDelete("{id}/allocation/{userId}")]
        [AuthorizePermission("PLAN04")]
        public async Task<ActionResult<BaseResponse>> DeleteAllocation(long id, long userId)
        {
            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }

            var user = await _userService.FindById(userId);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }
            else
            {
                if (user.role_id != (long)RoleEnum.MEMBER)
                {
                    return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
                }
            }

            var userPlan = await _planService.FindByUserPlanId(plan.id, userId);
            if (userPlan is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            } else
            {
                var hasPermission = await _planService.HasPermissionDeleteAllocation(id, userId);
                if (!hasPermission)
                {
                    return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ApiResource.UserPlanConditionChanged);
                }
            }

            try
            {
                await _planService.RemoveAllocationByPlanId(plan.id, userId);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/last-activity")]
        public async Task<ActionResult<BaseResponse>> LastActivity(long id, [FromQuery] LogActivityParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }

            var user = await _userService.FindById(request.user_id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var userPlan = await _planService.FindByUserPlanId(plan.id, request.user_id);
            if (userPlan is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            }

            try
            {
                var lastActivity = await _planService.GetLastActivity(plan.id, user.id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: lastActivity);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/log-activity")]
        public async Task<ActionResult<BaseResponse>> LogActivity(long id, [FromQuery] LogActivityParamDto request)
        {
            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }

            var user = await _userService.FindById(request.user_id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            try
            {
                var logs = await _planService.GetLogActivity(plan.id, request.user_id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: logs);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/activity")]
        public async Task<ActionResult<BaseResponse>> DetailActivity(long id, [FromQuery] LogActivityParamDto request)
        {
            var plan = await _planService.FindById(id);
            if (plan is null || plan.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.PlanNotFound);
            }

            var user = await _userService.FindById(request.user_id);
            if (user is null || user.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserNotFound);
            }

            var userPlan = await _planService.FindByUserPlanId(plan.id, request.user_id);
            if (userPlan is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            }

            try
            {
                var dataActivity = await _planService.GetDetailPlanActivityByUser(plan.id, request.user_id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: dataActivity);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }
    }
}
