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

        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public HealthController(
            ISettingService settingService,
            DataContext context,
            IConfiguration configuration
        )
        {
            _log = LogManager.GetLogger(typeof(HealthController));
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("")]
        public async Task<ActionResult<BaseResponse>> Get()
        {
            try
            {
                var errors = new List<string>();
                var checks = new Dictionary<string, object?>();

                var dbHealthy = await _context.Database.CanConnectAsync();
                checks["database"] = dbHealthy;
                if (!dbHealthy)
                {
                    errors.Add("Database connection failed.");
                }

                if (errors.Count == 0)
                {
                    return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: checks);
                }

                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError, ErrorDetails: errors, Data: checks);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

    }
}
