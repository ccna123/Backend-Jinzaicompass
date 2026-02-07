using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Attributes;
using SystemBrightSpotBE.Dtos.Setting;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.SettingService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : BaseController
    {
        private readonly ILog _log;
        private readonly ISettingService _settingService;
        
        public SettingController(
            ISettingService settingService
        )
        {
            _log = LogManager.GetLogger(typeof(SettingController));
            _settingService = settingService;
        }

        [Authorize]
        [HttpGet("")]
        [AuthorizePermission("SETTING01")]
        public async Task<ActionResult<BaseResponse>> Get()
        {
            try
            {
                var setting = await _settingService.GetSetting();

                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: setting);
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
        [HttpPut("")]
        [AuthorizePermission("SETTING02")]
        public async Task<ActionResult<BaseResponse>> Update([FromForm] UpdateSettingDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                bool removeFile = Request.Headers["Remove-File"].FirstOrDefault() == "1";
                await _settingService.UpdateSetting(request, removeFile);

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
