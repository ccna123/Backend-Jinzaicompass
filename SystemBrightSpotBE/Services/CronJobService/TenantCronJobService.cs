using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.CronJobService
{
    public class TenantCronJobService : ICronJobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        // Job run with 9:30AM JP 30 9 * * *
        // Job run with 5 minute */5 * * * *
        public TenantCronJobService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IWebHostEnvironment env
        ) : base("30 9 * * *", TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"))
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _env = env;
        }

        public override async Task DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var _authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                DateOnly currDay = DateOnly.FromDateTime(DateTime.Today);

                var tenants = await _context.tenants.Where(t => t.status != (long)TenantStatusEnum.EXPIRED).ToListAsync();

                foreach (var t in tenants)
                {
                    if (t.end_date < currDay)
                    {
                        t.status = (long)TenantStatusEnum.EXPIRED;
                        continue;
                    } 
                    
                    if (t.status == (long)TenantStatusEnum.SCHEDULED && t.start_date <= currDay)
                    {
                        Console.WriteLine("Send mail & change status tenant");
                        if (t.send_mail == false)
                        {
                            t.send_mail = true;
                            t.send_at = DateTime.Now;

                            // Gen password new
                            var password = _authService.GenerateSecurePassword();
                            var passwordHashed = new PasswordHasher();
                            var user = await _context.users.FirstOrDefaultAsync(u => u.email == t.email);

                            if (user == null)
                            {
                                throw new Exception("No user found for tenant email");
                            }

                            user.password = passwordHashed.HashPassword(password);
                            user.temp_password_used = true;
                            user.temp_password_expired_at = DateTime.Now.AddMinutes(10);

                            // Send mail
                            await SendMail(t, password);
                        }
                        // Update status to actived
                        t.status = (long)TenantStatusEnum.ACTIVED;
                        continue;
                    }

                    if (t.status == (long)TenantStatusEnum.ACTIVED || t.status == (long)TenantStatusEnum.SUSPENDED || t.status == (long)TenantStatusEnum.RENEWAL_DUE)
                    {
                        int remainDay = t.end_date.DayNumber - currDay.DayNumber;
                        Console.WriteLine($"Check remain day: {remainDay}");

                        if (new[] { 30, 7, 1 }.Contains(remainDay))
                        {
                            await SendMailWhenExpiring(t, remainDay);
                        }

                        if (remainDay <= 30 && t.status == (long)TenantStatusEnum.ACTIVED)
                        {
                            t.status = (long)TenantStatusEnum.RENEWAL_DUE;
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task SendMail(Tenant tenant, string password)
        {
            var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;

            var placeholders = new Dictionary<string, string>
                {
                    { "Name", tenant.name },
                    { "Password", password },
                    { "Website", urlFE },
                    { "Email", tenant.email}
                };

            // Initialize SQS client without credentials (use IAM Role), production enviroment
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
            string queueUrl = _configuration.GetSection("SQS:TenantQueueUrl").Value ?? String.Empty;
            var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
            var messageGroupId = _configuration.GetSection("SQS:Tenant").Value ?? String.Empty;

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

            string messageBody = JsonSerializer.Serialize(placeholders);

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageGroupId = messageGroupId,
            };

            await sqsClient.SendMessageAsync(sendMessageRequest);
        }

        private async Task SendMailWhenExpiring(Tenant tenant, int remainDay)
        {
            var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;
            string type = "AFTER1DAY";

            if (remainDay == 7)
            {
                type = "AFTER7DAY";
            }

            if (remainDay == 30)
            {
                type = "AFTER30DAY";
            }

            var placeholders = new Dictionary<string, string>
                {
                    { "Name", tenant.name },
                    { "Email", tenant.email },
                    { "EndDate", tenant.end_date.ToString("yyyy/MM/dd") ?? "" },
                    { "Type", type },
                };

            // Initialize SQS client without credentials (use IAM Role), production enviroment
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
            string queueUrl = _configuration.GetSection("SQS:TenantExpiringQueueUrl").Value ?? String.Empty;
            var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
            var messageGroupId = _configuration.GetSection("SQS:TenantExpiring").Value ?? String.Empty;

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
