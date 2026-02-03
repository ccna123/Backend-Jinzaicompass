using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Company;
using SystemBrightSpotBE.Dtos.MonitoringSystem;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.MonitoringSystemService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/monitoring-system")]
    [ApiController]
    public class MonitoringSystemController : BaseController
    {
        private readonly ILog _log;
        private DataContext _context;
        private IAuthService _authService;
        private readonly IMonitoringSystemService _monitoringSystemService;
        public MonitoringSystemController(
            DataContext context,
            IAuthService authService,
            IMonitoringSystemService monitoringSystemService
        ) {
            _log = LogManager.GetLogger(typeof(MonitoringSystemController));
            _context = context;
            _authService = authService;
            _monitoringSystemService = monitoringSystemService;
        }

        [Authorize]
        [HttpGet()]
        [AuthorizePermission("MONITORINGSYSTEM01")]
        public async Task<ActionResult<BaseResponse>> GetAll()
        {
            try
            {
                var monitoringsystems = await _monitoringSystemService.GetAll();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: monitoringsystems);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("create")]
        [AuthorizePermission("MONITORINGSYSTEM02")]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateMonitoringSystemDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            bool checkEmailExist = await _monitoringSystemService.CheckEmailExist(request.email);
            if (checkEmailExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: MonitoringSystemResource.EmailExist);
            }

            try
            {
                await _monitoringSystemService.Create(request);
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
        [AuthorizePermission("MONITORINGSYSTEM03")]
        public async Task<ActionResult<BaseResponse>> Delete(long id)
        {
            var company = await _monitoringSystemService.FindById(id);
            if (company is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: MonitoringSystemResource.MonitoringSystemNotFound);
            }

            try
            {
                await _monitoringSystemService.Delete(id);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        [AuthorizePermission("MONITORINGSYSTEM04")]
        public async Task<ActionResult<BaseResponse>> ChangePassword(MonitoringSystemChangePasswordDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var accountId = _authService.GetAccountId("Id");
            var account = _context.users.Where(u => u.id == accountId).FirstOrDefault();
            if (account is null)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ApiResource.AccoutNotExist);
            }

            var inCorrectPassword = _monitoringSystemService.IsCurrentPasswordValid(account, request.current_password);

            if (!inCorrectPassword)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: MonitoringSystemResource.CurrentPasswordIncorrect);
            }

            try
            {
                await _monitoringSystemService.ChangePassword(account, request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

    }
}
