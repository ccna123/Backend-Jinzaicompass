using Amazon.S3;
using Amazon.S3.Model;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Dtos.Report;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.CategoryService;
using SystemBrightSpotBE.Services.ReportService;

namespace SystemBrightSpotBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : BaseController
    {
        private readonly ILog _log;
        private readonly IReportService _reportService;
        private readonly ICategoryService _categoryService;
        private readonly IAmazonS3 _s3;

        public ReportController(
            IReportService reportService,
            ICategoryService categoryService,
            IAmazonS3 s3
        )
        {
            _log = LogManager.GetLogger(typeof(ReportController));
            _reportService = reportService;
            _categoryService = categoryService;
            _s3 = s3;
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
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
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
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
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
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
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
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");
                _log.Error(ex.ToString());
                return JJsonResponse(StatusCodes.Status500InternalServerError, ErrorMessage: ServerResource.InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> RequestPdfDownload(long id)
        {
            // 1. Kiểm tra report tồn tại và chưa bị xóa
            var report = await _reportService.FindById(id);
            if (report == null || report.deleted_at != null)
                return NotFound();

            // 2. Kiểm tra quyền xem
            bool hasPermissionView = await _reportService.HasPermisstionView(id);
            if (!hasPermissionView)
                return Forbid();

            try
            {
                Console.WriteLine($"Request download PDF for report ID: {id}");

                // 3. Gọi service mới: gửi SQS + lưu DynamoDB pending
                var sessionId = await _reportService.RequestReportDownloadAsync(id);

                // 4. Trả về session_id ngay lập tức (frontend sẽ poll status)
                return Ok(new
                {
                    sessionId,
                    message = "Yêu cầu tải PDF đã được gửi. Vui lòng chờ xử lý..."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR in RequestPdfDownload =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                _log.Error($"RequestPdfDownload failed for reportId={id}. Error: {ex.Message}", ex);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("report-status/{sessionId}")]
        public async Task<IActionResult> GetReportStatus(string sessionId)
        {
            try
            {
                // Giả sử bạn có method trong service để lấy từ DynamoDB
                var (status, url, updatedAt) = await _reportService.GetReportDownloadStatusAsync(sessionId);
                if (status == null)
                    return NotFound();

                return Ok(new
                {
                    status,
                    url,
                    updatedAt
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR in RequestPdfDownload =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");
                _log.Error($"GetReportStatus failed for sessionId={sessionId}. Error: {ex.Message}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
