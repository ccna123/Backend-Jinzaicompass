using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Models;

namespace SystemBrightSpotBE.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        ) {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetToken(User account)
        {
            if (_configuration.GetSection("AppSettings:Token").Value is null)
            {
                throw new Exception("AppSettings Token is null!");
            }
            if (_configuration.GetSection("AppSettings:Expires").Value is null)
            {
                throw new Exception("AppSettings Expires is null!");
            }
            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
            var expiresConfig = _configuration.GetSection("AppSettings:Expires")?.Value;
            int appSettingsExpires = 0;
            if (expiresConfig != null)
            {
                appSettingsExpires = int.Parse(expiresConfig);
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.id.ToString()),
                new Claim(ClaimTypes.Email, account.email.ToString()),
                new Claim(ClaimTypes.Role, account.role_id.ToString() ?? ""),
                new Claim("Department", account.department_id?.ToString() ?? ""),
                new Claim("Division", account.division_id?.ToString() ?? ""),
                new Claim("Group", account.group_id?.ToString() ?? ""),
                new Claim("FullName", $"{account.last_name} {account.first_name}"),
                new Claim("Password", account.password?.ToString() ?? ""),
                new Claim("Tenant", account.tenant_id?.ToString() ?? "")
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken != null ? appSettingsToken : ""));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            DateTime expires = DateTime.Now.AddDays(appSettingsExpires);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool Login(string email, string password)
        {
            var passwordHashed = new PasswordHasher();
            var account = _context.users.Where(u => u.email == email).FirstOrDefault();

            if (account is null)
            {
                return false;
            }
            else
            {
                if (passwordHashed.VerifyHashedPassword(account.password, password) == PasswordVerificationResult.Failed)
                {
                    return false;
                }
                else
                {
                    // Check status tenant & role
                    if (account.role_id != (long)RoleEnum.SUPPER_ADMIN)
                    { 
                        if (account.Tenant == null || account.Tenant.status != (long)TenantStatusEnum.ACTIVED)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public long GetAccountId(string type)
        {
            string? value = null;
            switch (type)
            {
                case "Id":
                    value = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    break;
                case "Department":
                    value = _httpContextAccessor.HttpContext!.User.FindFirstValue("Department");
                    break;
                case "Division":
                    value = _httpContextAccessor.HttpContext!.User.FindFirstValue("Division");
                    break;
                case "Group":
                    value = _httpContextAccessor.HttpContext!.User.FindFirstValue("Group");
                    break;
                case "Role":
                    value = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Role);
                    break;
                case "Tenant":
                    value = _httpContextAccessor.HttpContext!.User.FindFirstValue("Tenant");
                    break;
                default:
                    break;
            }

            var result = 0L;
            if (value != null && value != "")
            {
                result = long.Parse(value);
            }
            return result;
        }

        public string GetAccountFullName()
        {
            return _httpContextAccessor.HttpContext!.User.FindFirstValue("FullName") ?? String.Empty;
        }

        public async Task<bool> ResetPassword(long user_id)
        {
            var account = _context.users.Where(u => u.id == user_id).FirstOrDefault();
            if (account is null)
            {
                return false;
            }

            var newPassword = GenerateSecurePassword();
            if (String.IsNullOrEmpty(newPassword))
            {
                newPassword = GenerateSecurePassword();
            }
            var passwordHashed = new PasswordHasher();
            account.password = passwordHashed.HashPassword(newPassword);
            account.temp_password_used = true;
            account.temp_password_expired_at = DateTime.Now.AddMinutes(10);
            _context.users.Update(account);
            await _context.SaveChangesAsync();

            var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;

            var placeholders = new Dictionary<string, string>
            {
                { "Name", $"{account.last_name} {account.first_name}" },
                { "Password", newPassword },
                { "Website", urlFE },
                { "Email", account.email ?? String.Empty},
            };

            // Initialize SQS client without credentials (use IAM Role), production enviroment
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
            string queueUrl = _configuration.GetSection("SQS:ForgetPasswordQueueUrl").Value ?? String.Empty;
            var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
            var messageGroupId = _configuration.GetSection("SQS:ForgetPassword").Value ?? String.Empty;

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

            return true;
        }

        public string GenerateSecurePassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            const string allChars = upper + lower + digits + special;

            int passwordLength = Math.Max((int)AuthEnum.PASSWORD_MIN_LENGTH, 8);

            string passwordNew;

            do
            {
                var passwordChars = new List<char>
                {
                    GetRandomChar(upper),
                    GetRandomChar(lower),
                    GetRandomChar(digits),
                    GetRandomChar(special)
                };

                while (passwordChars.Count < passwordLength)
                {
                    passwordChars.Add(GetRandomChar(allChars));
                }

                passwordNew = new string(
                    passwordChars.OrderBy(_ => GetRandomInt(0, 1000)).ToArray()
                );

            } while (passwordNew.Length < passwordLength);

            return passwordNew;
        }

        private static char GetRandomChar(string chars)
        {
            byte[] buffer = new byte[1];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return chars[buffer[0] % chars.Length];
        }

        private static int GetRandomInt(int min, int max)
        {
            byte[] buffer = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            int value = BitConverter.ToInt32(buffer, 0);
            return Math.Abs(value % (max - min)) + min;
        }

        public async Task<bool> ChangePassword(long user_id, string new_password)
        {
            var account = _context.users.Where(u => u.id == user_id).FirstOrDefault();
            if (account is null)
            {
                return false;
            }
            var passwordHashed = new PasswordHasher();
            account.password = passwordHashed.HashPassword(new_password);

            account.temp_password_used = false;
            account.active = true;
            account.temp_password_expired_at = null;
            _context.users.Update(account);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
