using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Dtos.Plan.UserPlanCondition;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.UserPlanConditionService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/user-plan-condition")]
    [ApiController]
    public class UserPlanConditionController : BaseController
    {
        private readonly ILog _log;
        private readonly IUserPlanConditionService _userPlanConditionService;
        
        public UserPlanConditionController(
            IUserPlanConditionService userPlanConditionService
        )
        {
            _log = LogManager.GetLogger(typeof(UserPlanConditionController));
            _userPlanConditionService = userPlanConditionService;
        }

        [Authorize]
        [HttpGet("{id}/activity")]
        public async Task<ActionResult<BaseResponse>> Get(long id)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var userPlanCondition = await _userPlanConditionService.FindById(id);
            if (userPlanCondition == null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            }

            try
            {
                var activity = await _userPlanConditionService.GetActivity(id);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: activity);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("{id}/activity/create")]
        public async Task<ActionResult<BaseResponse>> Create(long id, [FromForm] CreateConditionActivityDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var userPlanCondition = await _userPlanConditionService.FindById(id);
            if (userPlanCondition == null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            }

            var hasPermisstion = await _userPlanConditionService.HasPermisstion(id, request.type);
            if (!hasPermisstion)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                await _userPlanConditionService.CreateActivity(id, request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("activity/{activityId}/update")]
        public async Task<ActionResult<BaseResponse>> Update(long activityId, [FromBody] UpdateConditionActivityDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var userPlanActivity = await _userPlanConditionService.FindByActivityId(activityId);
            if (userPlanActivity == null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanActivityNotFound);
            }

            var hasPermisstionActivity = await _userPlanConditionService.HasPermisstionByActivity(activityId, request.type);
            if (!hasPermisstionActivity)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                await _userPlanConditionService.UpdateActivity(activityId, request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }
    }
}
