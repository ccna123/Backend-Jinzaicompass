using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Report;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.NotificationService;
using SystemBrightSpotBE.Services.ReportService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly ILog _log;
        private readonly INotificationService _notificationService;
        private readonly IReportService _reportService;

        public NotificationController(
            INotificationService notificationService,
            IReportService reportService
        )
        {
            _log = LogManager.GetLogger(typeof(NotificationController));
            _notificationService = notificationService;
            _reportService = reportService;
        }

        [Authorize]
        [HttpGet("")]

        public async Task<ActionResult<BaseResponse>> Get()
        {
            try
            {
                var noties = await _notificationService.GetAll();
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: noties);
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
        [HttpPut("{reportId}/read")]
        public async Task<ActionResult<BaseResponse>> Read(long reportId)
        {
            var report = await _reportService.FindById(reportId);
            if (report is null || report.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ReportNotFound);
            }

            var notification = await _notificationService.FindByReportId(reportId);
            if (notification is null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.NotificatioNotFound);
            }

            try
            {
                await _notificationService.ReadByReportId(reportId);
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
