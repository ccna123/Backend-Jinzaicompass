using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using log4net;
using Microsoft.AspNetCore.Mvc;
using SystemBrightSpotBE.Data;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.SettingService;
using System.Linq;

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

                var s3Result = await CheckS3Async();
                checks["s3"] = s3Result;
                if (s3Result is not null)
                {
                    errors.Add($"S3: {s3Result}");
                }

                var sqsResult = await CheckSqsAsync();
                checks["sqs"] = sqsResult;
                if (sqsResult is not null)
                {
                    errors.Add($"SQS: {sqsResult}");
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

        private async Task<string?> CheckS3Async()
        {
            string regionName = (_configuration.GetSection("AWS:Region").Value ?? Environment.GetEnvironmentVariable("AWS_REGION") ?? String.Empty).Trim();
            string bucketName = _configuration.GetSection("AWS:BucketName").Value ?? String.Empty;
            string bucketNamePdf = _configuration.GetSection("AWS:BucketNameforPDF").Value ?? String.Empty;
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;

            if (string.IsNullOrWhiteSpace(bucketName) && string.IsNullOrWhiteSpace(bucketNamePdf))
            {
                return "BucketName/BucketNameforPDF is not configured.";
            }

            IAmazonS3 s3Client;
            if (!String.IsNullOrEmpty(accessKey) && !String.IsNullOrEmpty(secretKey))
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(regionName));
            }
            else
            {
                s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(regionName));
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(bucketName))
                {
                    await s3Client.GetBucketLocationAsync(new GetBucketLocationRequest { BucketName = bucketName });
                }

                if (!string.IsNullOrWhiteSpace(bucketNamePdf) && !string.Equals(bucketNamePdf, bucketName, StringComparison.Ordinal))
                {
                    await s3Client.GetBucketLocationAsync(new GetBucketLocationRequest { BucketName = bucketNamePdf });
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        private async Task<string?> CheckSqsAsync()
        {
            string regionName = _configuration.GetSection("SQS:SystemName").Value ?? String.Empty;
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;

            var queueUrls = new[]
            {
                _configuration.GetSection("SQS:UserQueueUrl").Value,
                _configuration.GetSection("SQS:ForgetPasswordQueueUrl").Value,
                _configuration.GetSection("SQS:ContactQueueUrl").Value,
                _configuration.GetSection("SQS:TenantQueueUrl").Value,
                _configuration.GetSection("SQS:TenantSettingQueueUrl").Value,
                _configuration.GetSection("SQS:TenantStatusQueueUrl").Value,
                _configuration.GetSection("SQS:TenantExpiringQueueUrl").Value
            }.Where(url => !string.IsNullOrWhiteSpace(url)).ToArray();

            if (queueUrls.Length == 0)
            {
                return "No SQS queue URLs configured.";
            }

            AmazonSQSClient sqsClient;
            if (!String.IsNullOrEmpty(accessKey) && !String.IsNullOrEmpty(secretKey))
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                sqsClient = new AmazonSQSClient(credentials, RegionEndpoint.GetBySystemName(regionName));
            }
            else
            {
                sqsClient = new AmazonSQSClient(RegionEndpoint.GetBySystemName(regionName));
            }

            try
            {
                foreach (var queueUrl in queueUrls)
                {
                    await sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
                    {
                        QueueUrl = queueUrl,
                        AttributeNames = new List<string> { "QueueArn" }
                    });
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }
    }
}
