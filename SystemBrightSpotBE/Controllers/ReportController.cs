using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Report;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.CategoryService;
using SystemBrightSpotBE.Services.ReportService;
using SystemBrightSpotBE.Services.S3Service;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : BaseController
    {
        private readonly ILog _log;
        private readonly IReportService _reportService;
        private readonly ICategoryService _categoryService;
        private readonly IS3Service _s3Service;

        public ReportController(
            IReportService reportService,
            ICategoryService categoryService,
            IS3Service s3Service
        )
        {
            _log = LogManager.GetLogger(typeof(ReportController));
            _reportService = reportService;
            _categoryService = categoryService;
            _s3Service = s3Service;
        }

        [Authorize]
        [HttpPost("")]
        public async Task<ActionResult<BaseResponse>> Search([FromBody] ListReportParamDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            try
            {
                var reports = await _reportService.Search(request);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: reports);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }


        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] CreateReportDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            if (request.report_type_id > 0)
            {
                var reportType = await _categoryService.FindById(request.report_type_id, CategoryTypeEnum.report_type);
                if (reportType is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            try
            {
                await _reportService.Create(request);
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
        public async Task<ActionResult<BaseResponse>> Detail(long id)
        {
            var report = await _reportService.FindById(id);
            if (report is null || report.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ReportNotFound);
            }

            bool hasPermissionView = await _reportService.HasPermisstionView(id);
            if (!hasPermissionView)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            try
            {
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success, Data: report);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpPut("{id}/update")]
        public async Task<ActionResult<BaseResponse>> Update(long id, [FromBody] UpdateReportDto request)
        {
            if (!ModelState.IsValid)
            {
                return JJsonResponse(StatusCodes.Status400BadRequest, ErrorMessage: ServerResource.BadRequest, ErrorDetails: ModelState);
            }

            if (request.report_type_id > 0)
            {
                var reportType = await _categoryService.FindById(request.report_type_id, CategoryTypeEnum.report_type);
                if (reportType is null)
                {
                    return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.CategoryNotFound);
                }
            }

            bool hasPermission = await _reportService.HasPermisstion(id);
            if (!hasPermission)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var report = await _reportService.FindById(id);
            if (report is null || report.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ReportNotFound);
            }

            try
            {
                await _reportService.Update(report.id, request);
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
        public async Task<ActionResult<BaseResponse>> Delete(long id)
        {
            bool hasPermission = await _reportService.HasPermisstion(id);
            if (!hasPermission)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var report = await _reportService.FindById(id);
            if (report is null || report.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ReportNotFound);
            }

            try
            {
                await _reportService.Delete(report.id);
                return JJsonResponse(StatusCodes.Status200OK, Message: ServerResource.Success);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DowloadPDF(long id)
        {
            var report = await _reportService.FindById(id);
            if (report is null || report.deleted_at != null)
            {
                return JJsonResponse(StatusCodes.Status404NotFound, Message: ApiResource.ReportNotFound);
            }

            bool hasPermissionView = await _reportService.HasPermisstionView(id);
            if (!hasPermissionView)
            {
                return JJsonResponse(StatusCodes.Status403Forbidden, ErrorMessage: ServerResource.Forbidden);
            }

            var userAgent = Request.Headers["User-Agent"].ToString();

            try
            {
                var data = await _reportService.DowloadPDF(id);
                var pdfBytes = await _reportService.ConvertHtmlToPDF(data);
                var fileName = $"{id}_レポート_{data.date:yyyyMMdd}.pdf";

                Response.Headers["Content-Disposition"] =
                    $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";
                _log.Info($"[PDF] User-Agent: {userAgent}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError,
                                     ErrorMessage: ServerResource.InternalServerError);
            }
        }


    }
}
