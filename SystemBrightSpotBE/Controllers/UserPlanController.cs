using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Plan.UserPlan;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.UserPlanService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/user-plan")]
    [ApiController]
    public class UserPlanController : BaseController
    {
        private readonly ILog _log;
        private readonly IUserPlanService _userPlanService;
        
        public UserPlanController(
            IUserPlanService userPlanService
        )
        {
            _log = LogManager.GetLogger(typeof(UserPlanController));
            _userPlanService = userPlanService;
        }

        [Authorize]
        [HttpGet("{id}/activity")]
        public async Task<ActionResult<BaseResponse>> Get(long id)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var userPlan = await _userPlanService.FindById(id);
            if (userPlan == null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            }

            try
            {
                var activity = await _userPlanService.GetActivity(id);

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
        public async Task<ActionResult<BaseResponse>> Create(long id, [FromBody] CreateActivityDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var userPlan = await _userPlanService.FindById(id);
            if (userPlan == null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanNotFound);
            }

            var hasPermisstion = await _userPlanService.HasPermisstionByUserPlan(id, request.type);
            if (!hasPermisstion) {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                await _userPlanService.CreateActivity(id, request);

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
        public async Task<ActionResult<BaseResponse>> Update(long activityId, [FromBody] UpdateActivityDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var userPlanActivity = await _userPlanService.FindByActivityId(activityId);
            if (userPlanActivity == null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.UserPlanActivityNotFound);
            }

            var hasPermisstionActivity = await _userPlanService.HasPermisstionByUserPlanActivity(activityId, request.type);
            if (!hasPermisstionActivity)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                await _userPlanService.UpdateActivity(activityId, request);

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
