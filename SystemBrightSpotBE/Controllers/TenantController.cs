using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Tenant;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.TenantService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : BaseController
    {
        private readonly ILog _log;
        private readonly ITenantService _tenantService;

        public TenantController(
            ITenantService tenantService
        )
        {
            _log = LogManager.GetLogger(typeof(TenantController));
            _tenantService = tenantService;
        }

        [Authorize]
        [HttpGet("")]
        [AuthorizePermission("TENANT01")]
        public async Task<ActionResult<BaseResponse>> Get([FromQuery] TenantParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var tenants = await _tenantService.GetAll(request);

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: tenants);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPost("create")]
        [AuthorizePermission("TENANT02")]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateTenantDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            bool checkEmailExist = await _tenantService.CheckEmailExist(request.email);
            if (checkEmailExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: TenantResource.EmailExist);
            }

            try
            {
                var tenant = await _tenantService.Create(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: tenant);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update")]
        [AuthorizePermission("TENANT03")]
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] UpdateTenantDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var tenant = await _tenantService.FindById(id);
            if (tenant is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CompanyNotFound);
            }

            bool checkEmailExist = await _tenantService.CheckEmailExist(request.email, update: true, id);
            if (checkEmailExist)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: TenantResource.EmailExist);
            }

            try
            {
                await _tenantService.Update(id, request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/detail")]
        [AuthorizePermission("TENANT04")]
        public async Task<ActionResult<BaseResponse>> Detail(long id)
        {
            var tenant = await _tenantService.FindById(id);
            if (tenant is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: AuthResource.TenantNotFound);
            }

            return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: tenant);
        }

        [Authorize]
        [HttpPut("{id}/update-expired")]
        [AuthorizePermission("TENANT03")]
        public async Task<ActionResult<BaseResponse>> UpdateExpried(long id, [FromBody] UpdateExpiredTenantDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var tenant = await _tenantService.FindById(id);
            if (tenant is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: AuthResource.TenantNotFound);
            }

            CheckExpiredDto result = await _tenantService.CheckChangeExpried(id, request.end_date);
            if (result.isvalid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: result.message);
            }

            try
            {
                await _tenantService.UpdateExpired(id, request.end_date);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update-status")]
        [AuthorizePermission("TENANT03")]
        public async Task<ActionResult<BaseResponse>> UpdateStatus(long id, [FromBody] UpdateStatusTenantDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            var tenant = await _tenantService.FindById(id);
            if (tenant is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: AuthResource.TenantNotFound);
            }

            bool result = await _tenantService.CheckChangeStatus(id, request.action);
            if (!result)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                await _tenantService.UpdateStatus(id, request.action);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        // ===============================
        // Used to verify tenant contract status.
        // ===============================
        [AllowAnonymous]
        [HttpPost("internal/contract-status")]
        public async Task<ActionResult<BaseResponse>> GetContractStatus()
        {
            try
            {
                var result = await _tenantService.GetTenantContractStatus();

                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                var executedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

                return JJsonResponse(
                    StatusCodes.Status200OK,
                    Message: ServerResource.Success,
                    Data: new
                    {
                        executed_at = executedAt,
                        total = result.Count,
                        items = result
                    }
                );
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(
                    StatusCodes.Status500InternalServerError,
                    ErrorMessage: ServerResource.InternalServerError
                );
            }
        }
    }
}
