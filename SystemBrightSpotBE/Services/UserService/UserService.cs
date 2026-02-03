
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Json;
using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.User;
using SystemBrightSpotBE.Dtos.UserCertification;
using SystemBrightSpotBE.Dtos.UserCompanyAward;
using SystemBrightSpotBE.Dtos.UserManager;
using SystemBrightSpotBE.Dtos.UserProject;
using SystemBrightSpotBE.Dtos.UserSkill;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Filters;
using SystemBrightSpotBE.Helpers;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Resources;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.S3Service;

namespace SystemBrightSpotBE.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
        private readonly IAuthService _authService;
        private readonly IS3Service _s3Service;
        private readonly IWebHostEnvironment _env;

        public UserService(
            IMapper mapper,
            DataContext context,
            IConfiguration configuration,
            IAuthService authService,
            IS3Service s3Service,
            IWebHostEnvironment env
        )
        {
            _configuration = configuration;
            _log = LogManager.GetLogger(typeof(UserService));
            _context = context;
            _authService = authService;
            _mapper = mapper;
            _s3Service = s3Service;
            _env = env;
        }

        public async Task<PagedResponse<List<UserListDto>>> GetAll([FromBody] ListUserParamDto request)
        {
            var paginationFilter = new PaginationFilter(request.page, request.size);
            var sortFilter = new SortFilter(request.order, request.column);
            var userId = _authService.GetAccountId("Id");
            List<long> userIds = await GetManagedUsersId();
            var dataQuery = _context.users.Where(u => userIds.Contains(u.id) && u.deleted_at == null).AsQueryable();

            if (!string.IsNullOrEmpty(request?.full_name))
            {
                string fullName = request.full_name.ToLower();
                dataQuery = dataQuery.Where(u =>
                    (u.last_name + u.first_name).ToLower().Contains(fullName) || (u.last_name_kana + u.first_name_kana).ToLower().Contains(fullName)
                );
            }

            if (request?.gender_id != null)
            {
                dataQuery = dataQuery.Where(u => u.gender_id == request.gender_id);
            }

            if (request?.certification_id != null)
            {
                dataQuery = dataQuery.Where(u => u.UserCertification != null && u.UserCertification.Any(uc => uc.certification_id == request.certification_id));
            }

            if (request?.employment_type_id != null)
            {
                dataQuery = dataQuery.Where(u => u.employment_type_id == request.employment_type_id);
            }

            var totalRecords = await dataQuery.CountAsync();
            var data = dataQuery
               .Select(result => new UserListDto
               {
                   id = result.id,
                   first_name = result.first_name,
                   last_name = result.last_name,
                   code = result.code,
                   department_name = result.Department != null ? result.Department.name : "",
                   division_name = result.Division != null ? result.Division.name : "",
                   group_name = result.Group != null ? result.Group.name : "",
                   position_name = result.Position != null ? result.Position.name : "",
                   employment_type_name = result.EmploymentType != null ? result.EmploymentType.name : "",
                   date_joining_company = result.UserStatusHistory != null ?
                                          result.UserStatusHistory.Where(h => h.type == 1).OrderBy(h => h.date).Select(h => (DateOnly?)h.date).FirstOrDefault() : null,
                   employment_status_name = result.EmploymentStatus != null ? result.EmploymentStatus.name : "",
                   active = result.active,
                   updated_at = result.updated_at
               }).AsQueryable();

            //Sort
            if (!string.IsNullOrEmpty(sortFilter.SortColumn))
            {
                if (sortFilter.SortBy == "desc")
                {
                    data = data.OrderByDescending(u => EF.Property<string>(u, sortFilter.SortColumn));
                }
                else
                {
                    data = data.OrderBy(u => EF.Property<string>(u, sortFilter.SortColumn));
                }
            }

            data = data.OrderBy(u => Convert.ToInt64(DataContext.RegexReplace(u.code, "[^0-9]", "", "g")));

            data = data.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize).Take(paginationFilter.PageSize);

            return PaginationHelper.CreatePagedResponse(await data.ToListAsync(), paginationFilter, totalRecords, null);
        }

        public async Task<List<long>> GetManagedUsersId(long? id = null)
        {
            var result = new List<long>();
            
            var userId = _authService.GetAccountId("Id");
            var tenantId = _authService.GetAccountId("Tenant");
            if (id != null && id > 0)
            {
                userId = (long)id;
            }

            var user = await _context.users.FindAsync(userId);

            if (user is not null && tenantId != 0)
            {
                switch (user.role_id)
                {
                    case (long)RoleEnum.SYSTEM_ADMIN:
                        result = await _context.users.Where(u => u.deleted_at == null)
                            .Where(u => u.tenant_id == tenantId)
                            .Select(u => u.id)
                            .ToListAsync();
                        break;
                    case (long)RoleEnum.POWER_USER:
                        result = await _context.users
                            .Where(u => u.deleted_at == null)
                            .Where(u => u.tenant_id == tenantId)
                            .Where(u => u.role_id == (long)RoleEnum.SENIOR_USER ||
                                   u.role_id == (long)RoleEnum.CONTRIBUTOR ||
                                   u.role_id == (long)RoleEnum.MEMBER)
                            .Where(u => u.department_id == user.department_id)
                            .Select(u => u.id)
                            .ToListAsync();
                        break;
                    case (long)RoleEnum.SENIOR_USER:
                        result = await _context.users
                            .Where(u => u.deleted_at == null)
                            .Where(u => u.tenant_id == tenantId)
                            .Where(u => u.role_id == (long)RoleEnum.CONTRIBUTOR || u.role_id == (long)RoleEnum.MEMBER)
                            .Where(u => u.department_id == user.department_id)
                            .Where(u => u.division_id == user.division_id)
                            .Select(u => u.id)
                            .ToListAsync();
                        break;
                    case (long)RoleEnum.CONTRIBUTOR:
                        result = await _context.users
                            .Where(u => u.deleted_at == null)
                            .Where(u => u.tenant_id == tenantId)
                            .Where(u => u.role_id == (long)RoleEnum.MEMBER)
                            .Where(u => u.department_id == user.department_id)
                            .Where(u => u.division_id == user.division_id)
                            .Where(u => u.group_id == user.group_id)
                            .Select(u => u.id)
                            .ToListAsync();
                        break;
                    default:
                        break;
                }

                result.Add(user.id);
            }

            return result.Distinct().ToList();
        }

        public async Task Create(AddUserDto request)
        {
            try
            {
                request.division_id = request.division_id != 0 ? request.division_id : null;
                request.group_id = request.group_id != 0 ? request.group_id : null;
                // Mapper user account
                var userCreate = _mapper.Map<User>(request);
                // Upload avatar and get path url
                if (request.avatar != null)
                {
                    var resultUpload = await _s3Service.UploadFileAsync(request.avatar, width: 150);
                    if (resultUpload != null)
                    {
                        userCreate.avatar = resultUpload.ToString();
                    }
                }
                var password = _authService.GenerateSecurePassword();
                var passwordHashed = new PasswordHasher();
                userCreate.password = passwordHashed.HashPassword(password);
                userCreate.temp_password_used = true;
                userCreate.temp_password_expired_at = DateTime.Now.AddMinutes(10);
                userCreate.created_at = DateTime.Now;
                userCreate.updated_at = DateTime.Now;

                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    await _context.users.AddAsync(userCreate);
                    await _context.SaveChangesAsync();

                    if (request.status_history != null && request.status_history.Any())
                    {
                        var historyEntities = request.status_history.Select(h => new UserStatusHistory
                        {
                            user_id = userCreate.id,
                            type = h.type,
                            date = h.date
                        }).ToList();

                        await _context.user_status_history.AddRangeAsync(historyEntities);
                        await _context.SaveChangesAsync();
                    }
                    // Commit transaction
                    await transaction.CommitAsync();
                }

                // Send mail
                var urlFE = _configuration.GetSection("AppSettings:UrlFrontend").Value ?? String.Empty;

                var placeholders = new Dictionary<string, string>
                {
                    { "Name", $"{userCreate.last_name} {userCreate.first_name}" },
                    { "Password", password },
                    { "Website", urlFE },
                    { "Email", userCreate.email ?? String.Empty}
                };

                // Initialize SQS client without credentials (use IAM Role), production enviroment
                string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
                string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;
                string queueUrl = _configuration.GetSection("SQS:UserQueueUrl").Value ?? String.Empty;
                var region = RegionEndpoint.GetBySystemName(_configuration.GetSection("SQS:SystemName").Value ?? String.Empty);
                var messageGroupId = _configuration.GetSection("SQS:RegisterUser").Value ?? String.Empty;

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
            catch (Exception ex)
            {
                _log.Error($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDto> FindById(long id)
        {
            var user = await _context.users
                    .Include(u => u.Gender)
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.Division)
                    .Include(u => u.Group)
                    .Include(u => u.Position)
                    .Include(u => u.EmploymentType)
                    .Include(u => u.EmploymentStatus)
                    .Include(u => u.UserStatusHistory)
                    .FirstOrDefaultAsync(u => u.id == id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> CheckExistAsync(string field, string value, bool update = false, long id = 0)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            bool result = false;
            if (update)
            {
                if (field == "mail")
                {
                    result = await _context.users.AnyAsync(u => u.email == value && u.id != id);
                }
                else if (field == "code")
                {
                    result = await _context.users.AnyAsync(u => u.code == value && u.tenant_id == tenantId && u.id != id);
                }
            }
            else
            {
                if (field == "mail")
                {
                    result = await _context.users.AnyAsync(u => u.email == value);

                }
                else if (field == "code")
                {
                    result = await _context.users.AnyAsync(u => u.code == value && u.tenant_id == tenantId);
                }
            }

            return result;
        }

        public async Task Update(long id, UpdateUserDto request, bool removeAvatar)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.users.FindAsync(id);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                // Delete file old
                string avatarOld = user.avatar ?? String.Empty;
                _mapper.Map(request, user);
                // Upload avatar and get path url
                if (request.avatar != null)
                {
                    if (!String.IsNullOrEmpty(avatarOld))
                    {
                        await _s3Service.DeleteFileAsync(avatarOld);
                    }
                    // Upload new file
                    var resultUpload = await _s3Service.UploadFileAsync(request.avatar, width: 150);
                    if (resultUpload != null)
                    {
                        user.avatar = resultUpload.ToString();
                    }
                }
                else
                {
                    user.avatar = avatarOld;

                    if (removeAvatar)
                    {
                        user.avatar = null;

                        if (!String.IsNullOrEmpty(avatarOld))
                        {
                            await _s3Service.DeleteFileAsync(avatarOld);
                        }
                    }
                }

                user.updated_at = DateTime.Now;

                _context.users.Update(user);
                await _context.SaveChangesAsync();

                if (request.status_history != null && request.status_history.Any())
                {
                    var oldHistories = _context.user_status_history.Where(h => h.user_id == id);
                    _context.user_status_history.RemoveRange(oldHistories);
                    await _context.SaveChangesAsync();

                    var newHistories = request.status_history.Select(h => new UserStatusHistory
                    {
                        user_id = id,
                        type = h.type,
                        date = h.date
                    }).ToList();

                    await _context.user_status_history.AddRangeAsync(newHistories);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Update User Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateGeneral(long id, UpdateUserGeneralDto request, bool removeAvatar)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.users.FindAsync(id);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                // Delete file old
                string avatarOld = user.avatar ?? String.Empty;
                _mapper.Map(request, user);
                // Upload avatar and get path url
                if (request.avatar != null)
                {
                    if (!String.IsNullOrEmpty(avatarOld))
                    {
                        await _s3Service.DeleteFileAsync(avatarOld);
                    }
                    // Upload new file
                    var resultUpload = await _s3Service.UploadFileAsync(request.avatar, width: 150);
                    if (resultUpload != null)
                    {
                        user.avatar = resultUpload.ToString();
                    }
                }
                else
                {
                    user.avatar = avatarOld;

                    if (removeAvatar)
                    {
                        user.avatar = null;

                        if (!String.IsNullOrEmpty(avatarOld))
                        {
                            await _s3Service.DeleteFileAsync(avatarOld);
                        }
                    }
                }

                user.updated_at = DateTime.Now;

                _context.users.Update(user);
                // Save change and commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Update User General Error: {ex.Message}");
                throw;
            }
        }

        public async Task Delete(long id)
        {
            var user = await _context.users
                .Include(u => u.UserStatusHistory)
                .Include(u => u.UserCertification)
                .Include(u => u.UserCompanyAward)
                .Include(u => u.UserExperienceJob)
                .Include(u => u.UserExperienceField)
                .Include(u => u.UserExperienceArea)
                .Include(u => u.UserSpecificSkill)
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(c => c.id == id);

            if (user is not null)
            {
                user.deleted_at = DateTime.Now;
                _context.users.Update(user);

                // History status
                if (user.UserStatusHistory?.Any() == true)
                {
                    _context.user_status_history.RemoveRange(user.UserStatusHistory);
                }

                // Certification
                if (user.UserCertification?.Any() == true)
                {
                    _context.user_certification.RemoveRange(user.UserCertification);
                }

                // Company award
                if (user.UserCompanyAward?.Any() == true)
                {
                    _context.user_company_award.RemoveRange(user.UserCompanyAward);
                }

                // Experience Job
                if (user.UserExperienceJob?.Any() == true)
                {
                    _context.user_experience_job.RemoveRange(user.UserExperienceJob);
                }

                // Experience Field
                if (user.UserExperienceField?.Any() == true)
                {
                    _context.user_experience_field.RemoveRange(user.UserExperienceField);
                }

                // Experience Area
                if (user.UserExperienceArea?.Any() == true)
                {
                    _context.user_experience_area.RemoveRange(user.UserExperienceArea);
                }

                // SpecificSkill
                if (user.UserSpecificSkill?.Any() == true)
                {
                    _context.user_specific_skill.RemoveRange(user.UserSpecificSkill);
                }

                // Delete related projects and their dependencies
                if (user.Projects?.Any() == true)
                {
                    foreach (var project in user.Projects)
                    {
                        if (project.ProjectExperienceJob?.Any() == true)
                        {
                            _context.project_experience_job.RemoveRange(project.ProjectExperienceJob);
                        }

                        if (project.ProjectExperienceField?.Any() == true)
                        {
                            _context.project_experience_field.RemoveRange(project.ProjectExperienceField);
                        }

                        if (project.ProjectExperienceArea?.Any() == true)
                        {
                            _context.project_experience_area.RemoveRange(project.ProjectExperienceArea);
                        }

                        if (project.ProjectSpecificSkill?.Any() == true)
                        {
                            _context.project_specific_skill.RemoveRange(project.ProjectSpecificSkill);
                        }

                        if (project.ProjectParticipationPosition?.Any() == true)
                        {
                            _context.project_participation_position.RemoveRange(project.ProjectParticipationPosition);
                        }

                        if (project.ProjectParticipationProcess?.Any() == true)
                        {
                            _context.project_participation_process.RemoveRange(project.ProjectParticipationProcess);
                        }

                        _context.projects.Remove(project);
                    }
                }

                // Report
                var reportByUserCreate = await _context.reports
                    .Where(r => r.user_id == user.id)
                    .ToListAsync();

                if (reportByUserCreate.Any())
                {
                    _context.reports.RemoveRange(reportByUserCreate);
                }

                var reportByUserAssign = await _context.report_users
                    .Where(r => r.user_id == user.id)
                    .ToListAsync();

                if (reportByUserAssign.Any())
                {
                    _context.report_users.RemoveRange(reportByUserAssign);
                }
                // Notification
                var notiByUser = await _context.notifications
                   .Where(r => r.user_id == user.id)
                   .ToListAsync();

                if (notiByUser.Any())
                {
                    _context.notifications.RemoveRange(notiByUser);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserGeneralDto?> FindByIdGeneral(long id)
        {
            var user = await _context.users
                .Where(u => u.id == id)
                .Select(u => new UserGeneralDto
                {
                    id = u.id,
                    avatar = u.avatar ?? String.Empty,
                    code = u.code,
                    first_name = u.first_name,
                    last_name = u.last_name,
                    gender_id = u.gender_id,
                    gender_name = u.Gender != null ? u.Gender.name : "",
                    date_of_birth = u.date_of_birth,
                    phone = u.phone,
                    address = u.address,
                    nearest_station = u.nearest_station,
                    department_name = u.Department != null ? u.Department.name : "",
                    employment_type_name = u.EmploymentType != null ? u.EmploymentType.name : "",
                    deleted_at = u.deleted_at
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<long?> FindRoleByUserId(long id)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.id == id);

            return user?.role_id;
        }

        public async Task<string?> FindPasswordByUserId(long id)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.id == id);

            return user?.password;
        }

        public async Task<BaseResponse> Import(IFormFile file)
        {
            var users = await ParseCsvAsync(file);
            var validListUser = new List<AddUserDto>();

            if (users.Count > 100)
            {
                return new BaseResponse
                {
                    Message = ServerResource.BadRequest,
                    ErrorMessage = ServerResource.BadRequest,
                    ErrorDetails = ApiResource.UserImportMax
                };
            }

            for (int i = 0; i < users.Count; i++)
            {
                var line = i + 2;

                var row = users[i];
                var dto = await MapToAddUserDtoAsync(row);
                var errors = ValidateDtoAsync(dto);

                if (errors.Any())
                {
                    return new BaseResponse
                    {
                        Message = ServerResource.BadRequest,
                        ErrorMessage = $"行 {line} にエラーがあります。",
                        ErrorDetails = errors.Select(e => e.ErrorMessage ?? "Unknown Error").ToList()
                    };
                }
                else
                {
                    // Check email, code exist
                    var checkEmailExists = await CheckExistAsync("mail", dto.email);
                    if (checkEmailExists)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.EmailExist
                        };
                    }

                    var checkCodeExists = await CheckExistAsync("code", dto.code);
                    if (checkCodeExists)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.CodeExist
                        };
                    }
                    // Check gender, role, department, division, position, group, employment type no exist
                    if (dto.gender_id == 0)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.GenderNotExist
                        };
                    }

                    if (dto.role_id == 0)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.RoleNotExist
                        };
                    }

                    if (dto.department_id == 0)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.DepartmentNotExist
                        };
                    }

                    //if (dto.division_id == 0)
                    //{
                    //    return new BaseResponse
                    //    {
                    //        Message = ServerResource.BadRequest,
                    //        ErrorMessage = $"行 {line} にエラーがあります。",
                    //        ErrorDetails = ApiResource.DivisionNotExist
                    //    };
                    //}

                    //if (dto.group_id == 0)
                    //{
                    //    return new BaseResponse
                    //    {
                    //        Message = ServerResource.BadRequest,
                    //        ErrorMessage = $"行 {line} にエラーがあります。",
                    //        ErrorDetails = ApiResource.GroupNotExist
                    //    };
                    //}

                    if (dto.position_id == 0)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.PositionNotExist
                        };
                    }

                    if (dto.employment_type_id == 0)
                    {
                        return new BaseResponse
                        {
                            Message = ServerResource.BadRequest,
                            ErrorMessage = $"行 {line} にエラーがあります。",
                            ErrorDetails = ApiResource.EmploymentTypeNotExist
                        };
                    }

                    // Get role, department, division, group by account
                    long roleId = _authService.GetAccountId("Role");
                    long departmentId = _authService.GetAccountId("Department");
                    long divisionId = _authService.GetAccountId("Division");
                    long groupId = _authService.GetAccountId("Group");

                    // Check role
                    if (roleId != (long)RoleEnum.SYSTEM_ADMIN && roleId != (long)RoleEnum.MEMBER)
                    {
                        if (dto.role_id <= roleId)
                        {
                            return new BaseResponse
                            {
                                Message = ServerResource.BadRequest,
                                ErrorMessage = $"行 {line} にエラーがあります。",
                                ErrorDetails = ApiResource.RoleInvalid
                            };
                        }
                    }
                    // Check map department, division, group
                    if (dto.group_id != 0)
                    {
                        var group = await _context.groups
                            .Include(g => g!.Department)
                            .FirstOrDefaultAsync(g => g.id == dto.group_id);

                        if (group == null || group.Department!.id != dto.department_id)
                        {
                            return new BaseResponse
                            {
                                Message = ServerResource.BadRequest,
                                ErrorMessage = $"行 {line} にエラーがあります。",
                                ErrorDetails = ApiResource.OrganizationInvalid
                            };
                        }
                    }

                    if (dto.division_id != 0)
                    {
                        var division = await _context.divisions
                            .Include(d => d!.Department)
                            .FirstOrDefaultAsync(d => d.id == dto.division_id);

                        if (division == null || division.Department!.id != dto.department_id)
                        {
                            return new BaseResponse
                            {
                                Message = ServerResource.BadRequest,
                                ErrorMessage = $"行 {line} にエラーがあります。",
                                ErrorDetails = ApiResource.OrganizationInvalid
                            };
                        }
                    }

                    // Check department, division, group by accout
                    switch (roleId)
                    {
                        case (long)RoleEnum.POWER_USER:
                            if (dto.department_id != departmentId)
                            {
                                return new BaseResponse
                                {
                                    Message = ServerResource.BadRequest,
                                    ErrorMessage = $"行 {line} にエラーがあります。",
                                    ErrorDetails = ApiResource.DepartmentInvalid
                                };
                            }
                            break;

                        case (long)RoleEnum.SENIOR_USER:
                            if (dto.department_id != departmentId)
                            {
                                return new BaseResponse
                                {
                                    Message = ServerResource.BadRequest,
                                    ErrorMessage = $"行 {line} にエラーがあります。",
                                    ErrorDetails = ApiResource.DepartmentInvalid
                                };
                            }

                            if (dto.division_id != divisionId)
                            {
                                return new BaseResponse
                                {
                                    Message = ServerResource.BadRequest,
                                    ErrorMessage = $"行 {line} にエラーがあります。",
                                    ErrorDetails = ApiResource.DivisionInvalid
                                };
                            }
                            break;

                        case (long)RoleEnum.CONTRIBUTOR:
                            if (dto.department_id != departmentId)
                            {
                                return new BaseResponse
                                {
                                    Message = ServerResource.BadRequest,
                                    ErrorMessage = $"行 {line} にエラーがあります。",
                                    ErrorDetails = ApiResource.DepartmentInvalid
                                };
                            }

                            if (dto.group_id != groupId)
                            {
                                return new BaseResponse
                                {
                                    Message = ServerResource.BadRequest,
                                    ErrorMessage = $"行 {line} にエラーがあります。",
                                    ErrorDetails = ApiResource.GroupInvalid
                                };
                            }
                            break;

                        default:
                            break;
                    }


                    validListUser.Add(dto);
                }
            }

            foreach (var user in validListUser)
            {
                try
                {
                    await Create(user);
                }
                catch (Exception ex)
                {
                    _log.Error($"Import user {user.email} failed: {ex.Message}");
                    //throw new Exception(ex.Message);
                }
            }

            return new BaseResponse { };
        }

        private async Task<AddUserDto> MapToAddUserDtoAsync(UserRowDto row)
        {
            return new AddUserDto
            {
                first_name = row.first_name ?? "",
                last_name = row.last_name ?? "",
                first_name_kana = row.first_name_kana ?? "",
                last_name_kana = row.last_name_kana ?? "",
                email = row.email ?? "",
                code = row.code ?? "",
                gender_id = (long)await MapNameToIdAsync(_context.genders, row.gender_name ?? ""),
                date_of_birth = row.date_of_birth ?? "",
                role_id = !String.IsNullOrEmpty(row.role_name) ? (long)await MapNameToIdAsync(_context.roles, row.role_name ?? "") : 5,
                department_id = (long)await MapNameToIdAsync(_context.departments, row.department_name ?? ""),
                division_id = (long)await MapNameToIdAsync(_context.divisions, row.division_name ?? ""),
                group_id = (long)await MapNameToIdAsync(_context.groups, row.group_name ?? ""),
                position_id = !String.IsNullOrEmpty(row.position_name) ? await MapNameToIdAsync(_context.positions, row.position_name ?? "") : null,
                employment_type_id = !String.IsNullOrEmpty(row.employment_type_name) ? await MapNameToIdAsync(_context.employment_types, row.employment_type_name ?? "") : null,
                phone = row.phone ?? "",
                address = row.address ?? "",
                nearest_station = row.nearest_station ?? ""
            };
        }

        private async Task<long?> MapNameToIdAsync<T>(DbSet<T> dbSet, string name) where T : class
        {
            try
            {
                var entity = await dbSet.FirstOrDefaultAsync(e => EF.Property<string>(e, "name") == name);

                if (entity != null)
                {
                    var idProp = entity.GetType().GetProperty("id");

                    return (long?)idProp?.GetValue(entity);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static List<ValidationResult> ValidateDtoAsync(object dto)
        {
            try
            {
                var context = new ValidationContext(dto, serviceProvider: null, items: null);
                var results = new List<ValidationResult>();

                Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<List<UserRowDto>> ParseCsvAsync(IFormFile file)
        {
            var users = new List<UserRowDto>();

            if (file == null || file.Length == 0)
            {
                return users;
            }

            using var stream = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            int lineNumber = 0;

            while (!stream.EndOfStream)
            {
                var line = await stream.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                lineNumber++;

                // Skip header
                if (lineNumber == 1 && line.Contains("姓"))
                {
                    continue;
                }

                var columns = line.Split(',');

                users.Add(new UserRowDto
                {
                    last_name = GetColumn(columns, 0),
                    first_name = GetColumn(columns, 1),
                    last_name_kana = GetColumn(columns, 2),
                    first_name_kana = GetColumn(columns, 3),
                    email = GetColumn(columns, 4),
                    code = GetColumn(columns, 5),
                    gender_name = GetColumn(columns, 6),
                    date_of_birth = GetColumn(columns, 7),
                    role_name = GetColumn(columns, 8),
                    department_name = GetColumn(columns, 9),
                    division_name = GetColumn(columns, 10),
                    group_name = GetColumn(columns, 11),
                    position_name = GetColumn(columns, 12),
                    employment_type_name = GetColumn(columns, 13),
                    phone = GetColumn(columns, 14),
                    address = GetColumn(columns, 15),
                    nearest_station = GetColumn(columns, 16)
                });
            }

            return users;
        }

        private string GetColumn(string[] cols, int index)
        {
            return index < cols.Length ? cols[index].Trim() : string.Empty;
        }

        public async Task<List<UserReportDto>> GetReportByTargetId(long id)
        {
            var user = await FindById(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var userId = user.id;
            var departmentId = user.department_id;
            var divisionId = user.division_id;
            var groupId = user.group_id;
            var roleId = user.role_id;
            var dataQuery = _context.reports
                .Select(r => new
                {
                    r.id,
                    r.title,
                    r.date,
                    r.is_public,
                    r.report_type_id,
                    r.user_id,
                    r.created_at,
                    ReportType = r.ReportType,
                    User = r.User,
                    ReportDepartment = r.ReportDepartment,
                    ReportDivision = r.ReportDivision,
                    ReportGroup = r.ReportGroup,
                    ReportUser = r.ReportUser
                })
                .AsQueryable();

            switch (roleId)
            {
                case (long)RoleEnum.MEMBER:
                    dataQuery = dataQuery.Where(r =>
                        r.is_public == true ||
                        r.user_id == userId ||
                        r.ReportUser!.Any(ru => ru.user_id == userId) ||
                        r.ReportDepartment!.Any(rd => rd.department_id == departmentId) ||
                        r.ReportDivision!.Any(rv => rv.division_id == divisionId) ||
                        r.ReportGroup!.Any(rg => rg.group_id == groupId)
                    );
                    break;
                case (long)RoleEnum.POWER_USER:
                case (long)RoleEnum.SENIOR_USER:
                case (long)RoleEnum.CONTRIBUTOR:
                    List<long> managerUserIds = await GetManagedUsersId(userId);
                    dataQuery = dataQuery.Where(r =>
                        r.is_public == true ||
                        managerUserIds.Contains(r.user_id) ||
                        r.ReportUser!.Any(ru => managerUserIds.Contains(ru.user_id)) ||
                        r.ReportDepartment!.Any(rd => rd.department_id == departmentId) ||
                        r.ReportDivision!.Any(rv => rv.division_id == divisionId) ||
                        r.ReportGroup!.Any(rg => rg.group_id == groupId)
                    );
                    break;
                case (long)RoleEnum.SYSTEM_ADMIN:
                    break;
            }

            return await dataQuery.Select(r => new UserReportDto {
                id = r.id,
                title = r.title,
                date = r.date,
                report_type_id = r.report_type_id,
                report_type_name = r.ReportType!.name,
                full_name = $"{r.User!.last_name} {r.User!.first_name}",
                created_at = r.created_at ?? DateTime.MinValue,
            })
            .OrderByDescending(r => r.date).ThenByDescending(r => r.created_at)
            .Skip(0)
            .Take(3)
            .ToListAsync();
        }

        public async Task<UserSkillDto> GetSkillByTargetId(long id)
        {
            var result = new UserSkillDto
            {
                user_id = id,

                experience_job = await _context.user_experience_job
                    .Where(x => x.user_id == id)
                    .Select(x => new UserExperienceJobDto
                    {
                        id = x.experience_job_id,
                        name = x.ExperienceJob != null ? x.ExperienceJob.name : ""
                    })
                    .ToListAsync(),

                experience_field = await _context.user_experience_field
                    .Where(x => x.user_id == id)
                    .Select(x => new UserExperienceFieldDto
                    {
                        id = x.experience_field_id,
                        name = x.ExperienceField != null ? x.ExperienceField.name : ""
                    })
                    .ToListAsync(),

                experience_area = await _context.user_experience_area
                    .Where(x => x.user_id == id)
                    .Select(x => new UserExperienceAreaDto
                    {
                        id = x.experience_area_id,
                        name = x.ExperienceArea != null ? x.ExperienceArea.name : ""
                    })
                    .ToListAsync(),

                specific_skill = await _context.user_specific_skill
                    .Where(x => x.user_id == id)
                    .Select(x => new UserSpecificSkillDto
                    {
                        id = x.specific_skill_id,
                        name = x.SpecificSkill != null ? x.SpecificSkill.name : ""
                    })
                    .ToListAsync(),
            };

            return result;
        }

        public async Task UpdateSkillByTargetId(long id, UpdateUserSkillDto request)
        {
            // Parse string to list
            var jobIds = ParseIds(request.experience_job);
            var fieldIds = ParseIds(request.experience_field);
            var areaIds = ParseIds(request.experience_area);
            var skillIds = ParseIds(request.specific_skill);

            // Delete all old user skill
            var oldJobs = _context.user_experience_job.Where(x => x.user_id == id);
            var oldFields = _context.user_experience_field.Where(x => x.user_id == id);
            var oldAreas = _context.user_experience_area.Where(x => x.user_id == id);
            var oldSkills = _context.user_specific_skill.Where(x => x.user_id == id);

            _context.user_experience_job.RemoveRange(oldJobs);
            _context.user_experience_field.RemoveRange(oldFields);
            _context.user_experience_area.RemoveRange(oldAreas);
            _context.user_specific_skill.RemoveRange(oldSkills);

            // Create new user skill
            var newJobs = jobIds.Select(jobId => new UserExperienceJob
            {
                user_id = id,
                experience_job_id = jobId
            });

            var newFields = fieldIds.Select(fieldId => new UserExperienceField
            {
                user_id = id,
                experience_field_id = fieldId
            });

            var newAreas = areaIds.Select(areaId => new UserExperienceArea
            {
                user_id = id,
                experience_area_id = areaId
            });

            var newSkills = skillIds.Select(skillId => new UserSpecificSkill
            {
                user_id = id,
                specific_skill_id = skillId
            });

            await _context.user_experience_job.AddRangeAsync(newJobs);
            await _context.user_experience_field.AddRangeAsync(newFields);
            await _context.user_experience_area.AddRangeAsync(newAreas);
            await _context.user_specific_skill.AddRangeAsync(newSkills);

            // Save change
            await _context.SaveChangesAsync();
        }

        private List<long> ParseIds(string ids)
        {
            return string.IsNullOrWhiteSpace(ids)
                ? new List<long>()
                : ids.Split(',')
                     .Select(s => s.Trim())
                     .Where(s => long.TryParse(s, out _))
                     .Select(long.Parse)
                     .Distinct()
                     .ToList();
        }

        public async Task<bool> CheckExistSkillAsync(string ids, SkillTypeEnum type)
        {
            var inputIds = ParseIds(ids);

            if (!inputIds.Any())
            {
                return true;
            }

            IQueryable<long> query = type switch
            {
                SkillTypeEnum.experience_job => _context.experience_jobs
                    .Where(x => inputIds.Contains(x.id)).Select(x => x.id),

                SkillTypeEnum.experience_field => _context.experience_fields
                    .Where(x => inputIds.Contains(x.id)).Select(x => x.id),

                SkillTypeEnum.experience_area => _context.experience_areas
                    .Where(x => inputIds.Contains(x.id)).Select(x => x.id),

                SkillTypeEnum.specific_skill => _context.specific_skills
                    .Where(x => inputIds.Contains(x.id)).Select(x => x.id),

                _ => Enumerable.Empty<long>().AsQueryable()
            };

            var existingIds = await query.ToListAsync();
            return !inputIds.Except(existingIds).Any();
        }

        public async Task<bool> CheckExistParticipationAsync(string ids, ParticipationTypeEnum type)
        {
            var inputIds = ParseIds(ids);

            if (!inputIds.Any())
            {
                return true;
            }

            IQueryable<long> query = type switch
            {
                ParticipationTypeEnum.participation_position => _context.participation_positions
                    .Where(x => inputIds.Contains(x.id)).Select(x => x.id),

                ParticipationTypeEnum.participation_process => _context.participation_processes
                    .Where(x => inputIds.Contains(x.id)).Select(x => x.id),

                _ => Enumerable.Empty<long>().AsQueryable()
            };

            var existingIds = await query.ToListAsync();
            return !inputIds.Except(existingIds).Any();
        }

        public async Task<List<UserCertificationDto>> GetCertificationByTargetId(long id)
        {
            return await _context.user_certification
                .Include(uc => uc.Certification)
                .Where(uc => uc.user_id == id)
                .OrderByDescending(uc => uc.certified_date)
                .Select(uc => new UserCertificationDto
                {
                    id = uc.id,
                    certification_id = uc.certification_id,
                    certification_name = uc.Certification != null ? uc.Certification.name : "",
                    certified_date = uc.certified_date,
                    user_id = uc.user_id
                })
                .ToListAsync();
        }

        public async Task CreateCertificationByTargetId(long id, CreateUserCertificationDto request)
        {
            var userCert = _mapper.Map<UserCertification>(request);
            userCert.user_id = id;

            _context.user_certification.Add(userCert);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCertificationByTargetId(long certificationId)
        {
            var userCerts = await _context.user_certification
                .Where(uc => uc.id == certificationId)
                .ToListAsync();

            if (!userCerts.Any())
            {
                return;
            }

            _context.user_certification.RemoveRange(userCerts);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserCompanyAwardDto>> GetCompanyAwardByTargetId(long id)
        {
            return await _context.user_company_award
                .Include(uc => uc.CompanyAward)
                .Where(uc => uc.user_id == id)
                .OrderByDescending(uc => uc.awarded_date)
                .Select(uc => new UserCompanyAwardDto
                {
                    id = uc.id,
                    company_award_id = uc.company_award_id,
                    company_award_name = uc.CompanyAward != null ? uc.CompanyAward.name : "",
                    awarded_date = uc.awarded_date,
                    user_id = uc.user_id
                })
                .ToListAsync();
        }

        public async Task CreateCompanyAwardByTargetId(long id, CreateUserCompanyAwardDto request)
        {
            var userComp = _mapper.Map<UserCompanyAward>(request);
            userComp.user_id = id;

            _context.user_company_award.Add(userComp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCompanyAwardByTargetId(long awardId)
        {
            var userComps = await _context.user_company_award
                .Where(uc => uc.id == awardId)
                .ToListAsync();

            if (!userComps.Any())
            {
                return;
            }

            _context.user_company_award.RemoveRange(userComps);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProjectDto> FindByProjectId(long id)
        {
            var project = await _context.projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.id == id);

            return _mapper.Map<UserProjectDto>(project);
        }

        public async Task<List<UserProjectDto>> GetProjectByTargetId(long id)
        {
            return await _context.projects
                .Where(p => p.user_id == id)
                .Include(p => p.Company)
                .Include(p => p.ProjectExperienceJob)
                    .ThenInclude(x => x.ExperienceJob)
                .Include(p => p.ProjectExperienceField)
                    .ThenInclude(x => x.ExperienceField)
                .Include(p => p.ProjectExperienceArea)
                    .ThenInclude(x => x.ExperienceArea)
                .Include(p => p.ProjectSpecificSkill)
                    .ThenInclude(x => x.SpecificSkill)
                .Include(p => p.ProjectParticipationPosition)
                    .ThenInclude(x => x.ParticipationPosition)
                .Include(p => p.ProjectParticipationProcess)
                    .ThenInclude(x => x.ParticipationProcess)
                .OrderByDescending(p => p.start_date)
                .Select(p => new UserProjectDto
                {
                    id = p.id,
                    name = p.name,
                    content = p.content,
                    description = p.description,
                    start_date = p.start_date,
                    end_date = p.end_date,
                    company_id = p.company_id,
                    company_name = p.Company != null ? p.Company.name : "",

                    experience_job = p.ProjectExperienceJob!.Select(j => new ProjectExperienceJobDto
                    {
                        id = j.experience_job_id,
                        name = j.ExperienceJob != null ? j.ExperienceJob.name : ""
                    }).ToList(),

                    experience_field = p.ProjectExperienceField!.Select(f => new ProjectExperienceFieldDto
                    {
                        id = f.experience_field_id,
                        name = f.ExperienceField != null ? f.ExperienceField.name : ""
                    }).ToList(),

                    experience_area = p.ProjectExperienceArea!.Select(a => new ProjectExperienceAreaDto
                    {
                        id = a.experience_area_id,
                        name = a.ExperienceArea != null ? a.ExperienceArea.name : ""
                    }).ToList(),

                    specific_skill = p.ProjectSpecificSkill!.Select(s => new ProjectSpecificSkillDto
                    {
                        id = s.specific_skill_id,
                        name = s.SpecificSkill != null ? s.SpecificSkill.name : ""
                    }).ToList(),

                    participation_position = p.ProjectParticipationPosition!.Select(pos => new ProjectParticipationPositionDto
                    {
                        id = pos.participation_position_id,
                        name = pos.ParticipationPosition != null ? pos.ParticipationPosition.name : ""
                    }).ToList(),

                    participation_process = p.ProjectParticipationProcess!.Select(proc => new ProjectParticipationProcessDto
                    {
                        id = proc.participation_process_id,
                        name = proc.ParticipationProcess != null ? proc.ParticipationProcess.name : ""
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task CreateProjectByTargetId(long id, CreateUserProjectDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Parse id to list id
                var jobIds = ParseIds(request.experience_job);
                var fieldIds = ParseIds(request.experience_field);
                var areaIds = ParseIds(request.experience_area);
                var skillIds = ParseIds(request.specific_skill);
                var positionIds = ParseIds(request.participation_position);
                var processIds = ParseIds(request.participation_process);

                // Create project
                var project = _mapper.Map<Project>(request);
                project.user_id = id;
                project.created_at = DateTime.Now;
                project.updated_at = DateTime.Now;

                // Save create project
                await _context.projects.AddAsync(project);
                await _context.SaveChangesAsync();

                // Set relation
                var experienceJobs = jobIds.Select(jobId => new ProjectExperienceJob
                {
                    project_id = project.id,
                    experience_job_id = jobId
                });

                var experienceFields = fieldIds.Select(fieldId => new ProjectExperienceField
                {
                    project_id = project.id,
                    experience_field_id = fieldId
                });

                var experienceAreas = areaIds.Select(areaId => new ProjectExperienceArea
                {
                    project_id = project.id,
                    experience_area_id = areaId
                });

                var specificSkills = skillIds.Select(skillId => new ProjectSpecificSkill
                {
                    project_id = project.id,
                    specific_skill_id = skillId
                });

                var participationPositions = positionIds.Select(posId => new ProjectParticipationPosition
                {
                    project_id = project.id,
                    participation_position_id = posId
                });

                var participationProcesses = processIds.Select(procId => new ProjectParticipationProcess
                {
                    project_id = project.id,
                    participation_process_id = procId
                });


                await _context.project_experience_job.AddRangeAsync(experienceJobs);
                await _context.project_experience_field.AddRangeAsync(experienceFields);
                await _context.project_experience_area.AddRangeAsync(experienceAreas);
                await _context.project_specific_skill.AddRangeAsync(specificSkills);
                await _context.project_participation_position.AddRangeAsync(participationPositions);
                await _context.project_participation_process.AddRangeAsync(participationProcesses);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _log.Error($"Create project failed: {ex}");

                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateProjectByTargetId(long projectId, UpdateUserProjectDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.projects.FirstOrDefaultAsync(p => p.id == projectId);
                if (project != null)
                {
                    _mapper.Map(request, project);
                    project.updated_at = DateTime.Now;
                    _context.projects.Update(project);

                    var oldJobs = _context.project_experience_job.Where(x => x.project_id == projectId);
                    var oldFields = _context.project_experience_field.Where(x => x.project_id == projectId);
                    var oldAreas = _context.project_experience_area.Where(x => x.project_id == projectId);
                    var oldSkills = _context.project_specific_skill.Where(x => x.project_id == projectId);
                    var oldPositions = _context.project_participation_position.Where(x => x.project_id == projectId);
                    var oldProcesses = _context.project_participation_process.Where(x => x.project_id == projectId);
                    _context.project_experience_job.RemoveRange(oldJobs);
                    _context.project_experience_field.RemoveRange(oldFields);
                    _context.project_experience_area.RemoveRange(oldAreas);
                    _context.project_specific_skill.RemoveRange(oldSkills);
                    _context.project_participation_position.RemoveRange(oldPositions);
                    _context.project_participation_process.RemoveRange(oldProcesses);

                    var jobIds = ParseIds(request.experience_job);
                    var fieldIds = ParseIds(request.experience_field);
                    var areaIds = ParseIds(request.experience_area);
                    var skillIds = ParseIds(request.specific_skill);
                    var positionIds = ParseIds(request.participation_position);
                    var processIds = ParseIds(request.participation_process);

                    var experienceJobs = jobIds.Select(jobId => new ProjectExperienceJob
                    {
                        project_id = project.id,
                        experience_job_id = jobId
                    });

                    var experienceFields = fieldIds.Select(fieldId => new ProjectExperienceField
                    {
                        project_id = project.id,
                        experience_field_id = fieldId
                    });

                    var experienceAreas = areaIds.Select(areaId => new ProjectExperienceArea
                    {
                        project_id = project.id,
                        experience_area_id = areaId
                    });

                    var specificSkills = skillIds.Select(skillId => new ProjectSpecificSkill
                    {
                        project_id = project.id,
                        specific_skill_id = skillId
                    });

                    var participationPositions = positionIds.Select(posId => new ProjectParticipationPosition
                    {
                        project_id = project.id,
                        participation_position_id = posId
                    });

                    var participationProcesses = processIds.Select(procId => new ProjectParticipationProcess
                    {
                        project_id = project.id,
                        participation_process_id = procId
                    });

                    await _context.project_experience_job.AddRangeAsync(experienceJobs);
                    await _context.project_experience_field.AddRangeAsync(experienceFields);
                    await _context.project_experience_area.AddRangeAsync(experienceAreas);
                    await _context.project_specific_skill.AddRangeAsync(specificSkills);
                    await _context.project_participation_position.AddRangeAsync(participationPositions);
                    await _context.project_participation_process.AddRangeAsync(participationProcesses);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Update project failed: {ex}");
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task DeleteProjectByTargetId(long projectId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.projects.FirstOrDefaultAsync(p => p.id == projectId);

                if (project != null)
                {
                    _context.projects.Remove(project);
                }

                await DeleteProjectRelations<ProjectExperienceJob>(_context.project_experience_job, projectId);
                await DeleteProjectRelations<ProjectExperienceField>(_context.project_experience_field, projectId);
                await DeleteProjectRelations<ProjectExperienceArea>(_context.project_experience_area, projectId);
                await DeleteProjectRelations<ProjectSpecificSkill>(_context.project_specific_skill, projectId);
                await DeleteProjectRelations<ProjectParticipationPosition>(_context.project_participation_position, projectId);
                await DeleteProjectRelations<ProjectParticipationProcess>(_context.project_participation_process, projectId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _log.Error($"Delete project failed: {ex}");
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task DeleteProjectRelations<T>(DbSet<T> dbSet, long projectId) where T : class
        {
            var items = await dbSet
                .Where(e => EF.Property<long>(e, "project_id") == projectId)
                .ToListAsync();

            dbSet.RemoveRange(items);
        }
        private int CalculateAge(DateOnly? birthDate)
        {
            if (birthDate != null)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                int? age = today.Year - birthDate?.Year;

                if (birthDate > today.AddYears((int)-age))
                {
                    age--;
                }

                return age ?? 0;
            }

            return 0;
        }

        public async Task<SkillSheetDto> GetSkillSheetByTargetId(long id)
        {
            var user = await FindById(id);
            var projects = await GetProjectByTargetId(id);
            var dataPDF = new SkillSheetDto
            {
                jid = $"J{user.id:D5}",
                full_name = $"{user.last_name} {user.first_name}",
                age = CalculateAge(user.date_of_birth),
                gender_name = user.gender_name ?? "",
                projects = projects,
            };

            return dataPDF;
        }

        public async Task<List<UserMemberDto>> GetMember(UserMemberParamDto request)
        {
            long tenantId = _authService.GetAccountId("Tenant");

            var result = await _context.users
                .Where(u => u.deleted_at == null)
                .Where(u => u.tenant_id == tenantId)
                .Where(u => u.role_id == (long)RoleEnum.MEMBER)
                .Where(u => u.department_id == request.department_id)
                .OrderBy(u => u.code)
                .Select(u => new UserMemberDto
                {
                    id = u.id,
                    first_name = u.first_name,
                    last_name = u.last_name,
                    department_id = u.department_id ?? 1,
                    division_id = u.division_id,
                    group_id = u.group_id
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<UserMemberDto>> GetTargetUsers()
        {
            long departmentId = _authService.GetAccountId("Department");
            long roleId = (long)_authService.GetAccountId("Role");
            long tenantId = _authService.GetAccountId("Tenant");

            var query = _context.users.Where(u => u.deleted_at == null).Where(u => u.tenant_id == tenantId).AsQueryable();
            query = query.OrderBy(u => u.code);

            return await query.Select(u => new UserMemberDto
            {
                id = u.id,
                first_name = u.first_name,
                last_name = u.last_name,
                department_id = u.department_id ?? 1,
                division_id = u.division_id,
                group_id = u.group_id
            }).ToListAsync();
        }

        public async Task<List<UserMemberDto>> GetUserByDepartment(long? departmentId)
        {
            long tenantId = _authService.GetAccountId("Tenant");
            if (departmentId == null)
            {
                departmentId = _authService.GetAccountId("Department");
            }
            
            var query = _context.users.Where(u => u.deleted_at == null).Where(u => u.tenant_id == tenantId).AsQueryable();
            
            long roleId = (long)_authService.GetAccountId("Role");
            if (roleId != (long)RoleEnum.SYSTEM_ADMIN)
            {
                query = query.Where(u => u.department_id == departmentId);
            }

            query = query.OrderBy(u => u.code);

            return await query.Select(u => new UserMemberDto
            {
                id = u.id,
                first_name = u.first_name,
                last_name = u.last_name,
                department_id = u.department_id ?? 1,
                division_id = u.division_id,
                group_id = u.group_id
            }).ToListAsync();
        }
    }
}