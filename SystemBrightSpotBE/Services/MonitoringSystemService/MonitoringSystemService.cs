using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SystemBrightSpotBE.Dtos.MonitoringSystem;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.MonitoringSystemService
{
    public class MonitoringSystemService : IMonitoringSystemService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
        private readonly IAuthService _authService;

        public MonitoringSystemService(
            IMapper mapper,
            DataContext context,
            IConfiguration configuration,
            IAuthService authService
        )
        {
            _configuration = configuration;
            _log = LogManager.GetLogger(typeof(MonitoringSystemService));
            _context = context;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<List<MonitoringSystemDto>> GetAll()
        {
            var data = await _context.monitoring_systems
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<MonitoringSystemDto>>(data);
        }

        public async Task Create(CreateMonitoringSystemDto request)
        {
            var monitoringSystem = _mapper.Map<MonitoringSystem>(request);

            _context.monitoring_systems.Add(monitoringSystem);
            await _context.SaveChangesAsync();
            // Send mail
            var emails = await GetMonitoringSystemEmails();

            await SendMailMonitoringSystem(emails, MonitoringSystemTypeEnum.CREATED, monitoringSystem);
        }

        public async Task Delete(long id)
        {
            var monitoringSystem = await _context.monitoring_systems.FindAsync(id);
            if (monitoringSystem == null)
            {
                throw new Exception("Monitoring System not found");
            }

            var emails = await GetMonitoringSystemEmails();

            _context.monitoring_systems.Remove(monitoringSystem);
            await _context.SaveChangesAsync();
            // Send mail
            await SendMailMonitoringSystem(emails, MonitoringSystemTypeEnum.DELETED, monitoringSystem);
        }

        public async Task<MonitoringSystemDto> FindById(long id)
        {
            var monitoringSystem = await _context.monitoring_systems
                .AsNoTracking()
                .FirstOrDefaultAsync(ms => ms.id == id);

            return _mapper.Map<MonitoringSystemDto>(monitoringSystem);
        }

        public async Task<bool> CheckEmailExist(string value, bool update, long id)
        {
            bool result = false;
            if (update)
            {
                result = await _context.monitoring_systems.AnyAsync(u => u.email == value && u.id != id);
            }
            else
            {
                result = await _context.monitoring_systems.AnyAsync(u => u.email == value);
            }

            return result;
        }

        public bool IsCurrentPasswordValid(User user, string currentPassword)
        {
            var passwordHasher = new PasswordHasher();

            var result = passwordHasher.VerifyHashedPassword(
                user.password,
                currentPassword
            );

            return result == PasswordVerificationResult.Success;
        }

        public async Task ChangePassword(User user, MonitoringSystemChangePasswordDto request)
        {
            var passwordHasher = new PasswordHasher();

            user.password = passwordHasher.HashPassword(request.new_password);

            _context.users.Update(user);
            await _context.SaveChangesAsync();
            // Send mail
            var emails = await GetMonitoringSystemEmails();

            await SendMailMonitoringSystem(emails, MonitoringSystemTypeEnum.CHANGED_PASSWORD);
        }
        private async Task<List<string>> GetMonitoringSystemEmails()
        {
            return await _context.monitoring_systems
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.email))
                .Select(x => x.email)
                .Distinct()
                .ToListAsync();
        }

        private async Task SendMailMonitoringSystem(List<string> toEmails, MonitoringSystemTypeEnum type, MonitoringSystem? monitoringSystem = null)
        {
            var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;
            var name = monitoringSystem != null ? $"{monitoringSystem.last_name} {monitoringSystem.first_name}" : "Administartor";
            var typeString = type switch
            {
                MonitoringSystemTypeEnum.CREATED => "CREATED",
                MonitoringSystemTypeEnum.DELETED => "DELETED",
                MonitoringSystemTypeEnum.CHANGED_PASSWORD => "CHANGED_PASSWORD",
                _ => type.ToString().ToUpper()
            };

            foreach (var email in toEmails)
            {
                var placeholders = new Dictionary<string, string>
                {
                    { "Name", name },
                    { "Email", monitoringSystem != null ? monitoringSystem.email : ""},
                    { "EmailTo", email ?? String.Empty},
                    { "Date", DateTime.Now.ToString("yyyy年MM月dd日 HH:mm") },
                    { "Type", typeString }
                };

                // Initialize SQS client without credentials (use IAM Role), production enviroment
                string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
                string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
                string queueUrl = _configuration.GetSection("SQS:TenantSettingQueueUrl").Value ?? String.Empty;
                var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
                var messageGroupId = _configuration.GetSection("SQS:TenantSetting").Value ?? String.Empty;

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
}
