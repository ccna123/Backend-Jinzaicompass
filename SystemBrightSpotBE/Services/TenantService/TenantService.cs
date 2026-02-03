using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;
using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.Tenant;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Filters;
using SystemBrightSpotBE.Helpers;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.TenantService
{
    public class TenantService : ITenantService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        public TenantService(
            IMapper mapper,
            DataContext context,
            IConfiguration configuration,
            IAuthService authService,
            IWebHostEnvironment env
        )
        {
            _configuration = configuration;
            _log = LogManager.GetLogger(typeof(TenantService));
            _context = context;
            _authService = authService;
            _mapper = mapper;
            _env = env;
        }

        public async Task<PagedResponse<List<TenantDto>>> GetAll(TenantParamDto request)
        {
            var paginationFilter = new PaginationFilter(request.page, request.size);
            var sortFilter = new SortFilter(request.order, request.column);

            var dataQuery = _context.tenants.AsQueryable();

            var totalRecords = await dataQuery.CountAsync();
            var data = dataQuery
               .Select(t => new TenantDto
               {
                   id = t.id,
                   name = t.name,
                   first_name = t.first_name,
                   last_name = t.last_name,
                   email = t.email,
                   phone = t.phone,
                   post_code = t.post_code,
                   region = t.region,
                   locality = t.locality,
                   street = t.street,
                   building_name = t.building_name,
                   comment = t.comment,
                   start_date = t.start_date,
                   end_date = t.end_date,
                   status = t.status,
                   send_mail = t.send_mail,
                   updated_at = t.updated_at ?? DateTime.MinValue,
                   created_at = t.created_at ?? DateTime.MinValue
               }).AsQueryable();

            data = data.OrderByDescending(t => t.created_at);

            data = data.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize).Take(paginationFilter.PageSize);

            return PaginationHelper.CreatePagedResponse(await data.ToListAsync(), paginationFilter, totalRecords, null);
        }

        public async Task<TenantDto> Create(CreateTenantDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create tenant
                var tenant = _mapper.Map<Tenant>(request);
                tenant.status = (long)TenantStatusEnum.IN_PREVIEW;
                tenant.updated_at = DateTime.Now;

                _context.tenants.Add(tenant);
                await _context.SaveChangesAsync();

                // Gen password
                var password = _authService.GenerateSecurePassword();
                var passwordHashed = new PasswordHasher();
                var code = await GenerateUserCode();

                // Create account user system admin
                var user = new User
                {
                    first_name = "アドミン",
                    last_name = "システム",
                    first_name_kana = "アドミン",
                    last_name_kana = "システム",
                    email = request.email,
                    code = code,
                    gender_id = 1,
                    role_id = 1,
                    tenant_id = tenant.id,
                    is_tenant_created = true,
                    active = true,
                    password = passwordHashed.HashPassword(password),
                    temp_password_used = true,
                    temp_password_expired_at = DateTime.Now.AddMinutes(10)
                };

                _context.users.Add(user);
                await _context.SaveChangesAsync();

                // Create data category by tenant
                string path = Path.Combine(
                    _env.ContentRootPath,
                    "Data",
                    "Database",
                    "Seeder",
                    "category.xlsx"
                );
                await SeedDataFromExcelByTenantAsync(path, tenant.id);
                // Commit transaction
                await transaction.CommitAsync();

                return _mapper.Map<TenantDto>(tenant);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Error create tenant: {ex.Message}");
                throw;
            }
        }

        private async Task<string> GenerateUserCode()
        {
            var count = await _context.users.CountAsync();
            var nextNumber = count + 1;

            return $"AB{nextNumber:D4}";
        }

        public async Task Update(long id, UpdateTenantDto request)
        {
            var tenant = await _context.tenants.FindAsync(id);
            if (tenant == null)
            {
                throw new Exception("Tenant not found");
            }

            var oldEmail = tenant.email;

            _mapper.Map(request, tenant);

            if (!string.Equals(oldEmail, tenant.email, StringComparison.OrdinalIgnoreCase))
            {
                // Get user system admin create to tenant
                var userSystemAdminByTenant = await _context.users
                    .Where(u => u.tenant_id == tenant.id && u.is_tenant_created == true && u.role_id == (long)RoleEnum.SYSTEM_ADMIN)
                    .FirstAsync();

                if (userSystemAdminByTenant != null)
                {
                    userSystemAdminByTenant.email = tenant.email;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateExpired(long id, DateOnly endDate)
        {
            var tenant = await _context.tenants.FindAsync(id);
            if (tenant == null)
            {
                throw new Exception("Tenant not found");
            }

            if (tenant.status == (long)TenantStatusEnum.EXPIRED)
            {
                tenant.status = (long)TenantStatusEnum.ACTIVED;
            }

            tenant.end_date = endDate;

            await _context.SaveChangesAsync();
            // Send mail notice
            await SendMailWhenChangeStatus(tenant, "RENEW");
        }

        public async Task UpdateStatus(long id, long action)
        {
            var tenant = await _context.tenants.FindAsync(id);
            if (tenant == null)
            {
                throw new Exception("Tenant not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                switch (action)
                {
                    case (long)TenantActionEnum.ACTIVE:
                        // Check status tenant and send mail
                        if (tenant.status == (long)TenantStatusEnum.IN_PREVIEW)
                        {
                            var today = DateOnly.FromDateTime(DateTime.Today);
                            if (tenant.start_date <= today)
                            {
                                if (tenant.send_mail == false)
                                {
                                    tenant.send_mail = true;
                                    tenant.send_at = DateTime.Now;

                                    // Gen password new
                                    var password = _authService.GenerateSecurePassword();
                                    var passwordHashed = new PasswordHasher();
                                    var user = await _context.users.FirstOrDefaultAsync(u => u.email == tenant.email);

                                    if (user == null)
                                    {
                                        throw new Exception("No user found for tenant email");
                                    }

                                    user.password = passwordHashed.HashPassword(password);
                                    user.temp_password_used = true;
                                    user.temp_password_expired_at = DateTime.Now.AddMinutes(10);

                                    // Send mail
                                    await SendMail(tenant, password);
                                }
                                // Update status to actived
                                tenant.start_date = today;
                                tenant.status = (long)TenantStatusEnum.ACTIVED;
                            }
                            else
                            {
                                // Update status to scheduled
                                tenant.status = (long)TenantStatusEnum.SCHEDULED;
                            }
                        }
                        else if (tenant.status == (long)TenantStatusEnum.SUSPENDED)
                        {
                            // Update status to actived
                            tenant.status = (long)TenantStatusEnum.ACTIVED;
                        }
                        else if (tenant.status == (long)TenantStatusEnum.SCHEDULED)
                        {
                            var today = DateOnly.FromDateTime(DateTime.Today);
                            tenant.send_mail = true;

                            // Gen password new
                            var password = _authService.GenerateSecurePassword();
                            var passwordHashed = new PasswordHasher();
                            var user = await _context.users.FirstOrDefaultAsync(u => u.email == tenant.email);

                            if (user == null)
                            {
                                throw new Exception("No user found for tenant email");
                            }

                            user.password = passwordHashed.HashPassword(password);
                            user.temp_password_used = true;
                            user.temp_password_expired_at = DateTime.Now.AddMinutes(10);

                            // Send mail
                            await SendMail(tenant, password);

                            // Update status to actived
                            tenant.start_date = today;
                            tenant.status = (long)TenantStatusEnum.ACTIVED;
                        }

                        break;
                    case (long)TenantActionEnum.SUSPEND:
                        tenant.status = (long)TenantStatusEnum.SUSPENDED;

                        // Send mail notice 
                        await SendMailWhenChangeStatus(tenant, "SUSPEND");
                        break;
                    case (long)TenantActionEnum.TERMINATE:
                        try
                        {
                            // Send mail notice
                            await SendMailWhenChangeStatus(tenant, "TERMINATE");

                            var users = _context.users.Where(u => u.tenant_id == id);
                            _context.users.RemoveRange(users);

                            _context.tenants.Remove(tenant);
                            break;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _log.Error($"Error terminate tenant: {ex.Message}");
                            throw;
                        }
                    default:
                        break;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to update tenant status: " + ex.Message);
            }
        }

        private async Task SendMail(Tenant tenant, string password)
        {
            var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;

            var placeholders = new Dictionary<string, string>
                {
                    { "Name", $"{tenant.last_name} {tenant.first_name}" },
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

        private async Task SendMailWhenChangeStatus(Tenant tenant, string type)
        {
            var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;

            var placeholders = new Dictionary<string, string>
                {
                    { "Name", tenant.name },
                    { "Email", tenant.email},
                    { "EndDate", tenant.end_date.ToString("yyyy/MM/dd") ?? "" },
                    { "Type", type }
                };

            // Initialize SQS client without credentials (use IAM Role), production enviroment
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
            string queueUrl = _configuration.GetSection("SQS:TenantStatusQueueUrl").Value ?? String.Empty;
            var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
            var messageGroupId = _configuration.GetSection("SQS:TenantStatus").Value ?? String.Empty;

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

        public async Task<bool> CheckEmailExist(string value, bool update, long id)
        {
            bool result = false;
            bool userExists = false;

            if (update)
            {
                result = await _context.tenants.AnyAsync(u => u.email == value && u.id != id);
                userExists = await _context.users.AnyAsync(u => u.email == value && u.is_tenant_created != true);
            }
            else
            {
                result = await _context.tenants.AnyAsync(u => u.email == value);
                userExists = await _context.users.AnyAsync(u => u.email == value);
            }

            return result || userExists;
        }

        public async Task<CheckExpiredDto> CheckChangeExpried(long id, DateOnly newEndDate)
        {
            var result = new CheckExpiredDto();

            var tenant = await _context.tenants.FindAsync(id);
            if (tenant == null)
            {
                result.isvalid = true;
                result.message = AuthResource.TenantNotFound;
                return result;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Check tenant status is In Preview
            if (tenant.status == (long)TenantStatusEnum.IN_PREVIEW)
            {
                //if (today > tenant.start_date)
                //{
                //    result.isvalid = true;
                //    result.message = TenantResource.StartDateMustBeTodayOrLater;
                //    return result;
                //}

                if (tenant.start_date > newEndDate || today > newEndDate)
                {
                    result.isvalid = true;
                    result.message = TenantResource.NewEndDateMustBeAfterStartDate;
                    return result;
                }
            }

            // Check tenant status is Actived or Expired
            if (tenant.status == (long)TenantStatusEnum.ACTIVED || tenant.status == (long)TenantStatusEnum.EXPIRED)
            {
                if (today > newEndDate)
                {
                    result.isvalid = true;
                    result.message = TenantResource.NewEndDateMustBeAfterToday;
                    return result;
                }
            }

            return result;
        }

        public async Task<bool> CheckChangeStatus(long id, long action)
        {
            var tenant = await _context.tenants.FindAsync(id);
            if (tenant == null)
            {
                throw new Exception("Tenant not found");
            }

            switch (tenant.status)
            {
                case (long)TenantStatusEnum.IN_PREVIEW:
                    if (action == (long)TenantActionEnum.SUSPEND)
                    {
                        return false;
                    }
                    break;
                case (long)TenantStatusEnum.SCHEDULED:
                    if (action == (long)TenantActionEnum.SUSPEND || action == (long)TenantActionEnum.RENEW)
                    {
                        return false;
                    }
                    break;
                case (long)TenantStatusEnum.ACTIVED:
                    if (action == (long)TenantActionEnum.ACTIVE)
                    {
                        return false;
                    }
                    break;
                case (long)TenantStatusEnum.SUSPENDED:
                    if (action == (long)TenantActionEnum.SUSPEND || action == (long)TenantActionEnum.RENEW)
                    {
                        return false;
                    }
                    break;
                case (long)TenantStatusEnum.EXPIRED:
                    if (action == (long)TenantActionEnum.ACTIVE || action == (long)TenantActionEnum.SUSPEND)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        public async Task<TenantDto> FindById(long id)
        {
            var tenant = await _context.tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(tena => tena.id == id);

            return _mapper.Map<TenantDto>(tenant);
        }

        private Dictionary<string, List<(string Text, string Color)>> ReadExcelCategoryFile(string path)
        {
            ExcelPackage.License.SetNonCommercialPersonal("SystemBrightSpotBE");
            var result = new Dictionary<string, List<(string Text, string Color)>>();

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int columnCount = worksheet.Dimension.End.Column;
                int rowCount = worksheet.Dimension.End.Row;

                var tableNames = new List<string>();
                for (int col = 1; col <= columnCount; col++)
                {
                    var tableName = worksheet.Cells[1, col].Text;
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tableNames.Add(tableName);
                        result[tableName] = new List<(string Text, string Color)>();
                    }
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    for (int col = 1; col <= tableNames.Count; col++)
                    {
                        var text = worksheet.Cells[row, col].Text;
                        var color = worksheet.Cells[row, col].Style.Font.Color.Rgb?.ToString() ?? String.Empty;
                        if (!string.IsNullOrEmpty(text))
                        {
                            result[tableNames[col - 1]].Add((text, color));
                        }
                    }
                }
            }

            return result;
        }

        private async Task SeedDataFromExcelByTenantAsync(string pathCategory, long tenantId)
        {

            try
            {
                var dataCategory = ReadExcelCategoryFile(pathCategory);

                if (dataCategory.ContainsKey("department"))
                {
                    foreach (var record in dataCategory["department"])
                    {
                        var department = new Department
                        {
                            name = record.Text,
                            delete_flag = !string.IsNullOrEmpty(record.Color),
                            tenant_id = tenantId
                        };
                        _context.departments.Add(department);
                    }
                    await _context.SaveChangesAsync();
                }
                // No seeding division and group
                //if (dataCategory.ContainsKey("division"))
                //{
                //    foreach (var record in dataCategory["division"])
                //    {
                //        var division = new Division
                //        {
                //            name = record.Text,
                //            department_id = 1,
                //            tenant_id = tenantId,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.divisions.Add(division);
                //    }
                //    await _context.SaveChangesAsync();
                //}

                //if (dataCategory.ContainsKey("group"))
                //{
                //    foreach (var record in dataCategory["group"])
                //    {
                //        var group = new Group
                //        {
                //            name = record.Text,
                //            division_id = 1,
                //            department_id = 1,
                //            tenant_id = tenantId,
                //            delete_flag = !string.IsNullOrEmpty(record.Color)
                //        };
                //        _context.groups.Add(group);
                //    }
                //    await _context.SaveChangesAsync();
                //}
                ExperienceJob? firstJob = null;
                ExperienceField? firstField = null;
                ExperienceArea? firstArea = null;

                if (dataCategory.ContainsKey("experience-job"))
                {
                    var jobs = new List<ExperienceJob>();

                    foreach (var record in dataCategory["experience-job"])
                    {
                        var experienceJob = new ExperienceJob
                        {
                            name = record.Text,
                            delete_flag = !string.IsNullOrEmpty(record.Color),
                            tenant_id = tenantId
                        };

                        jobs.Add(experienceJob);
                        _context.experience_jobs.Add(experienceJob);
                    }

                    await _context.SaveChangesAsync();
                    firstJob = jobs.FirstOrDefault();
                }

                if (firstJob != null && dataCategory.ContainsKey("experience-field"))
                {
                    var fields = new List<ExperienceField>();

                    foreach (var record in dataCategory["experience-field"])
                    {
                        var experienceField = new ExperienceField
                        {
                            name = record.Text,
                            experience_job_id = firstJob.id,
                            delete_flag = !string.IsNullOrEmpty(record.Color),
                            tenant_id = tenantId
                        };

                        fields.Add(experienceField);
                        _context.experience_fields.Add(experienceField);
                    }

                    await _context.SaveChangesAsync();
                    firstField = fields.FirstOrDefault();
                }

                if (firstField != null && dataCategory.ContainsKey("experience-area"))
                {
                    var areas = new List<ExperienceArea>();

                    foreach (var record in dataCategory["experience-area"])
                    {
                        var experienceArea = new ExperienceArea
                        {
                            name = record.Text,
                            experience_field_id = firstField.id,
                            delete_flag = !string.IsNullOrEmpty(record.Color),
                            tenant_id = tenantId
                        };

                        areas.Add(experienceArea);
                        _context.experience_areas.Add(experienceArea);
                    }

                    await _context.SaveChangesAsync();
                    firstArea = areas.FirstOrDefault();
                }

                if (firstArea != null && dataCategory.ContainsKey("specific-skill"))
                {
                    foreach (var record in dataCategory["specific-skill"])
                    {
                        var specificSkill = new SpecificSkill
                        {
                            name = record.Text,
                            experience_area_id = firstArea.id,
                            delete_flag = !string.IsNullOrEmpty(record.Color),
                            tenant_id = tenantId
                        };
                        _context.specific_skills.Add(specificSkill);
                    }
                    await _context.SaveChangesAsync();
                }

                foreach (var entry in dataCategory)
                {
                    var tableName = entry.Key;
                    var records = entry.Value;
                    var skipTable = new List<string> { "departmnet", "division", "group", "experience-job", "experience-field", "experience-area", "specific-skill" };
                    if (skipTable.Contains(tableName))
                    {
                        continue;
                    }

                    foreach (var record in records)
                    {
                        var text = record.Text;
                        var color = record.Color;
                        switch (tableName.ToLower())
                        {
                            case "certification":
                                var certification = new Certification
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.certifications.Add(certification);
                                break;

                            case "company-award":
                                var companyAward = new CompanyAward
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.company_awards.Add(companyAward);
                                break;

                            case "position":
                                var position = new Position
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.positions.Add(position);
                                break;

                            case "employment-type":
                                var employmentType = new EmploymentType
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.employment_types.Add(employmentType);
                                break;

                            case "employment-status":
                                //var employmentStatus = new EmploymentStatus
                                //{
                                //    name = text
                                //};
                                //_context.employment_status.Add(employmentStatus);
                                break;

                            case "gender":
                                //var gender = new Gender
                                //{
                                //    name = text
                                //};
                                //_context.genders.Add(gender);
                                break;

                            case "role":
                                //var role = new Role
                                //{
                                //    name = text
                                //};
                                //_context.roles.Add(role);
                                break;

                            case "participation-process":
                                var participationProcess = new ParticipationProcess
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.participation_processes.Add(participationProcess);
                                break;

                            case "participation-position":
                                var participationPosition = new ParticipationPosition
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.participation_positions.Add(participationPosition);
                                break;

                            case "report-type":
                                var reportType = new ReportType
                                {
                                    name = text,
                                    delete_flag = !string.IsNullOrEmpty(color),
                                    tenant_id = tenantId
                                };
                                _context.report_types.Add(reportType);
                                break;
                            default:
                                break;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _log.Info($"Data category seeded successfully with tenant id: {tenantId}");
            }
            catch (Exception ex)
            {
                _log.Error($"Error during seeding: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TenantContractStatusDto>> GetTenantContractStatus()
        {

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

            var today = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone)
            );

            var notifyDays = new[] { 1, 7, 30 };

            var targetEndDates = notifyDays
                .Select(d => today.AddDays(d))
                .ToList();

            var result = await _context.tenants
                .AsNoTracking()
                .Where(t =>
                    (t.status == (long)TenantStatusEnum.ACTIVED ||
                    t.status == (long)TenantStatusEnum.SUSPENDED)
                    && targetEndDates.Contains(t.end_date)
                )
                .Select(t => new TenantContractStatusDto
                {
                    tenant_id = t.id,
                    tenant_email = t.email,
                    remain_days = t.end_date.DayNumber - today.DayNumber
                })
                .ToListAsync();

            // var result = raw
            //     .Select(t =>
            //     {
            //         var remainDays = t.end_date.DayNumber - today.DayNumber;

            //         return new TenantContractStatusDto
            //         {
            //             tenant_id = t.id,
            //             tenant_email = t.email,
            //             remain_days = remainDays
            //         };
            //     })
            //     .Where(x => notifyDays.Contains(x.remain_days))
            //     .ToList();

            return result;
        }

    }
}
