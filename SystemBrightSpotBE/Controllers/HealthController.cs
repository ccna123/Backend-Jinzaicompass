using log4net;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.SettingService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : BaseController
    {
        private readonly ILog _log;
        
        public HealthController(
            ISettingService settingService
        )
        {
            _log = LogManager.GetLogger(typeof(HealthController));
        }

        [HttpGet("")]
        public Task<ActionResult<BaseResponse>> Get()
        {
            try
            {
                return Task.FromResult<ActionResult<BaseResponse>>(JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success));
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return Task.FromResult<ActionResult<BaseResponse>>(JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError));
            }
        }
    }
}
