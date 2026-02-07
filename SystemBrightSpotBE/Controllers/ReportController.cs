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
        public async Task<IActionResult> DowloadPDF(long id)
        {
            var report = await _reportService.FindById(id);
            if (report is null || report.deleted_at != null)
                return NotFound();

            bool hasPermissionView = await _reportService.HasPermisstionView(id);
            if (!hasPermissionView)
                return Forbid();

            try
            {
                Console.WriteLine("Step 1: load data");
                var data = await _reportService.DowloadPDF(id);

                Console.WriteLine("Step 2: generate pdf");
                var pdfBytes = await _reportService.ConvertHtmlToPDF(data);

                var fileName = $"{id}_レポート_{data.date:yyyyMMdd}.pdf";
                var key = $"app/pdf/{Guid.NewGuid()}_{fileName}";

                using var stream = new MemoryStream(pdfBytes);
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
                cts.CancelAfter(TimeSpan.FromSeconds(20));

                Console.WriteLine("Step 3: upload to s3");

                await _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = "jinzaicompass-pdf",
                    Key = key,
                    InputStream = stream,
                    ContentType = "application/pdf",
                    AutoCloseStream = false,
                    UseChunkEncoding = false,
                    Headers =
            {
                ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}"
            }
                }, cts.Token);

                Console.WriteLine("Step 4: generate presigned url");

                var presignRequest = new GetPreSignedUrlRequest
                {
                    BucketName = "jinzaicompass-pdf",
                    Key = key,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddMinutes(10)
                };

                var url = _s3.GetPreSignedURL(presignRequest);

                Console.WriteLine("Step 5: return url");

                return Ok(new
                {
                    url = url
                });
            }
            catch (OperationCanceledException ex)
            {
                var isRequestAborted = HttpContext.RequestAborted.IsCancellationRequested;
                var cancelReason = isRequestAborted ? "request_aborted" : "s3_upload_timeout";

                Console.WriteLine("===== ERROR =====");
                Console.WriteLine($"cancelReason: {cancelReason}");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");

                _log.Error($"DowloadPDF upload canceled. reportId={id}, reason={cancelReason}, exception={ex}");

                return StatusCode(StatusCodes.Status504GatewayTimeout, "Upload timeout");
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine("===== S3 ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("====================");

                _log.Error($"S3 upload failed. reportId={id}, ErrorCode={ex.ErrorCode}, StatusCode={ex.StatusCode}, RequestId={ex.RequestId}. {ex}");

                return StatusCode(StatusCodes.Status502BadGateway, "S3 upload failed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");

                _log.Error(ex.ToString());

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
