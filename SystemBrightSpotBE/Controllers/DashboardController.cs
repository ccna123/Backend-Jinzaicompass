using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.DashboardService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly ILog _log;
        private readonly IDashboardService _dashboardService;
        public DashboardController(
            IDashboardService dashboardService
        )
        {
            _log = LogManager.GetLogger(typeof(DashboardController));
            _dashboardService = dashboardService;
        }

        [Authorize]
        [HttpGet("user-by-month")]
        public async Task<ActionResult<BaseResponse>> CalculateUserByMonth()
        {
            try
            {
                var data = await _dashboardService.CalculateUserByMonth();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("user-by-year")]
        public async Task<ActionResult<BaseResponse>> CalculateUserByYear()
        {
            try
            {
                var data = await _dashboardService.CalculateUserByYear();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("user-recent")]
        public async Task<ActionResult<BaseResponse>> GetUserRecent()
        {
            try
            {
                var data = await _dashboardService.GetUserRecent();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("report-recent")]
        public async Task<ActionResult<BaseResponse>> GetReportRecent()
        {
            try
            {
                var data = await _dashboardService.GetReportRecent();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("participation-position")]
        public async Task<ActionResult<BaseResponse>> CalculateParticipationPositionRatio()
        {
            try
            {
                var data = await _dashboardService.CalculateParticipationPositionRatio();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("user-seniority")]
        public async Task<ActionResult<BaseResponse>> CalculateUserSeniority()
        {
            try
            {
                var data = await _dashboardService.CalculateUserSeniority();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("experience-job")]
        public async Task<ActionResult<BaseResponse>> CalculateExperienceJobRatio()
        {
            try
            {
                var data = await _dashboardService.CalculateExperienceJobRatio();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("experience-field")]
        public async Task<ActionResult<BaseResponse>> CalculateExperienceFieldRatio()
        {
            try
            {
                var data = await _dashboardService.CalculateExperienceFieldRatio();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("experience-area")]
        public async Task<ActionResult<BaseResponse>> CalculateExperienceAreaRatio()
        {
            try
            {
                var data = await _dashboardService.CalculateExperienceAreaRatio();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("specific-skill")]
        public async Task<ActionResult<BaseResponse>> CalculateSpecificSkillRatio()
        {
            try
            {
                var data = await _dashboardService.CalculateSpecificSkillRatio();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: data);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }
    }
}
