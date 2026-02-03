using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SystemBrightSpotBE.Dtos.Contact;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.ContactServices
{
    public class ContactService : IContactService
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public ContactService(
            DataContext context,
            IConfiguration configuration,
            IAuthService authService
        )
        {
            _configuration = configuration;
            _context = context;
            _authService = authService;
        }

        public async Task Create(CreateContactDto request)
        {
            string userFullName = _authService.GetAccountFullName();
            long tenantId = _authService.GetAccountId("Tenant");

            // Send all user with role system admin
            var adminEmails = await _context.users
                .Where(u => u.role_id == (long)RoleEnum.SYSTEM_ADMIN)
                .Where(u => u.tenant_id == tenantId)
                .Select(u => u.email)
                .ToListAsync();

            // Initialize SQS client without credentials (use IAM Role), production enviroment
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
            string queueUrl = _configuration.GetSection("SQS:ContactQueueUrl").Value ?? String.Empty;
            var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
            var messageGroupId = _configuration.GetSection("SQS:Contact").Value ?? String.Empty;

            AmazonSQSClient sqsClient;
            if (!String.IsNullOrEmpty(accessKey) && !String.IsNullOrEmpty(secretKey))
            {
                // use credentials
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                sqsClient = new AmazonSQSClient(credentials, region);
            }
            else
            {
                // use IAM Role
                sqsClient = new AmazonSQSClient(region);
            }

            foreach (var email in adminEmails)
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "FullName", userFullName },
                    { "Name", request.name },
                    { "FromEmail", request.email },
                    { "Phone", request.phone },
                    { "Content", request.content.Replace("\r\n", "<br>").Replace("\n", "<br>") },
                    { "Email", email ?? String.Empty},
                    { "Title", $"【{request.title}】について"},
                };

                string messageBody = JsonSerializer.Serialize(placeholders);

                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = messageBody,
                    MessageGroupId = messageGroupId,
                };

                await sqsClient.SendMessageAsync(sendMessageRequest);
            }
        }
    }
}
