using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using HtmlAgilityPack;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.Report;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Filters;
using SystemBrightSpotBE.Helpers;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.NotificationService;
using SystemBrightSpotBE.Services.UserService;

namespace SystemBrightSpotBE.Services.ReportService
{
    public class ReportService : IReportService
    {
        private readonly DataContext _context;
        private readonly ILog _log;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _sqsQueueUrl;
        private readonly string _dynamoTableName;
        public ReportService(
            DataContext context,
            IMapper mapper,
            IAuthService authService,
            IUserService userService,
            INotificationService notificationService,
            IAmazonSQS sqsClient,              
            IAmazonDynamoDB dynamoDbClient,    
            IConfiguration configuration       
        )
        {
            _context = context;
            _mapper = mapper;
            _log = LogManager.GetLogger(typeof(ReportService));
            _authService = authService;
            _userService = userService;
            
            _notificationService = notificationService;
            _sqsClient = sqsClient;
            _dynamoDbClient = dynamoDbClient;

            _sqsQueueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL")
            ?? configuration["AWS:SQS:QueueUrl"]
            ?? throw new InvalidOperationException("Thiếu cấu hình SQS_QUEUE_URL (cả env var và appsettings đều không có)");

            _dynamoTableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME")
                ?? "ReportDownloadStatus"; // fallback nếu không set
        }

        public async Task<PagedResponse<List<ReportDto>>> Search(ListReportParamDto request)
        {
            var paginationFilter = new PaginationFilter(request.page, request.size);
            var sortFilter = new SortFilter(request.order, request.column);

            long tenantId = _authService.GetAccountId("Tenant");

            var dataQuery = _context.reports
                .Where(r => r.tenant_id == tenantId)
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

            // Filter with param
            if (!string.IsNullOrEmpty(request?.title))
            {
                dataQuery = dataQuery.Where(r => r.title.Contains(request.title));
            }

            if (request?.report_type_id != null)
            {
                dataQuery = dataQuery.Where(r => r.report_type_id == request.report_type_id);
            }

            long userId = _authService.GetAccountId("Id");
            long departmentId = _authService.GetAccountId("Department");
            long divisionId = _authService.GetAccountId("Division");
            long groupId = _authService.GetAccountId("Group");
            long roleId = _authService.GetAccountId("Role");
            List<long> managerUserIds = await _userService.GetManagedUsersId();

            if (request?.viewer_id != null && request.viewer_id != userId)
            {
                var userViewer = await _context.users.Where(u => u.id == request.viewer_id).FirstOrDefaultAsync();

                if (userViewer == null) {
                    throw new Exception("User not found");
                }

                userId = userViewer.id;
                departmentId = userViewer.department_id ?? 1;
                divisionId = userViewer.division_id ?? 0;
                groupId = userViewer.group_id ?? 0;
                roleId = userViewer.role_id ?? 5;
                managerUserIds = await _userService.GetManagedUsersId(userId);
            }

            if (!string.IsNullOrEmpty(request?.user_ids))
            {
                var paramUserIds = ParseIds(request?.user_ids ?? String.Empty);
                // Search by user create report
                switch (roleId)
                {
                    case (long)RoleEnum.MEMBER:
                        dataQuery = dataQuery.Where(r =>
                            paramUserIds.Contains(r.user_id) && 
                            (
                                r.is_public == true ||
                                r.user_id == userId ||
                                r.ReportUser!.Any(ru => ru.user_id == userId) ||
                                r.ReportDepartment!.Any(rd => rd.department_id == departmentId) ||
                                r.ReportDivision!.Any(rv => rv.division_id == divisionId) ||
                                r.ReportGroup!.Any(rg => rg.group_id == groupId)
                            )
                        );
                        break;
                    case (long)RoleEnum.POWER_USER:
                    case (long)RoleEnum.SENIOR_USER:
                    case (long)RoleEnum.CONTRIBUTOR:
                        dataQuery = dataQuery.Where(r =>
                            paramUserIds.Contains(r.user_id) &&
                            (
                                r.is_public == true ||
                                r.user_id == userId ||
                                r.ReportUser!.Any(ru => managerUserIds.Contains(ru.user_id)) ||
                                r.ReportDepartment!.Any(rd => rd.department_id == departmentId) ||
                                r.ReportDivision!.Any(rv => rv.division_id == divisionId) ||
                                r.ReportGroup!.Any(rg => rg.group_id == groupId)
                            )
                        );
                        break;
                    case (long)RoleEnum.SYSTEM_ADMIN:
                        dataQuery = dataQuery.Where(r =>
                            paramUserIds.Contains(r.user_id)
                        );
                        break;
                }
            } 
            else
            {
                // Where by permission
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
            }

            var totalRecords = await dataQuery.CountAsync();
            var data = dataQuery
               .Select(r => new ReportDto
               {
                   id = r.id,
                   title = r.title,
                   content = String.Empty,
                   date = r.date,
                   is_public = r.is_public,
                   report_type_id = r.report_type_id,
                   report_type_name = r.ReportType!.name,
                   user_id = r.user_id,
                   user_fullname = $"{r.User!.last_name} {r.User!.first_name}",
                   created_at = r.created_at ?? DateTime.MinValue,
                   departments = r.ReportDepartment!.Select(rd => new ReportDepartmentDto
                   {
                       report_id = rd.report_id,
                       department_id = rd.department_id,
                       department_name = rd.Department!.name
                   }).ToList(),
                   divisions = r.ReportDivision!.Select(rd => new ReportDivisionDto
                   {
                       report_id = rd.report_id,
                       division_id = rd.division_id,
                       division_name = rd.Division!.name
                   }).ToList(),
                   groups = r.ReportGroup!.Select(rg => new ReportGroupDto
                   {
                       report_id = rg.report_id,
                       group_id = rg.group_id,
                       group_name = rg.Group!.name
                   }).ToList(),
                   users = r.ReportUser!.Select(ru => new ReportUserDto
                   {
                       report_id = ru.report_id,
                       user_id = ru.user_id,
                       user_fullname = $"{ru.User!.last_name} {ru.User!.first_name}"
                   }).ToList(),
               }).AsQueryable();

            data = data.OrderByDescending(r => r.date).ThenByDescending(r => r.created_at);

            data = data.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize).Take(paginationFilter.PageSize);

            return PaginationHelper.CreatePagedResponse(await data.ToListAsync(), paginationFilter, totalRecords, null);
        }

        public async Task Create(CreateReportDto request)
        {
            var userId = _authService.GetAccountId("Id");
            var departmentIds = new List<long>();
            var divisionIds = new List<long>();
            var groupIds = new List<long>();
            var userIds = new List<long>();
            // Parse string to list long
            if (!String.IsNullOrEmpty(request.department_ids))
            {
                departmentIds = ParseIds(request.department_ids ?? String.Empty);
            }
            if (!String.IsNullOrEmpty(request.division_ids))
            {
                divisionIds = ParseIds(request.division_ids ?? String.Empty);
            }
            if (!String.IsNullOrEmpty(request.group_ids))
            {
                groupIds = ParseIds(request.group_ids ?? String.Empty);
            }
            if (!String.IsNullOrEmpty(request.user_ids))
            {
                userIds = ParseIds(request.user_ids ?? String.Empty);
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create report
                var newReport = _mapper.Map<Report>(request);
                newReport.is_public = request.is_public;
                newReport.user_id = userId;
                await _context.reports.AddAsync(newReport);
                await _context.SaveChangesAsync();
                // Create relation
                if (departmentIds.Any())
                {
                    _context.report_departments.AddRange(departmentIds.Select(id => new ReportDepartment { report_id = newReport.id, department_id = id }));
                }

                if (divisionIds.Any())
                {
                    _context.report_divisions.AddRange(divisionIds.Select(id => new ReportDivision { report_id = newReport.id, division_id = id }));
                }

                if (groupIds.Any())
                {
                    _context.report_groups.AddRange(groupIds.Select(id => new ReportGroup { report_id = newReport.id, group_id = id }));
                }

                if (userIds.Any())
                {
                    _context.report_users.AddRange(userIds.Select(id => new ReportUser { report_id = newReport.id, user_id = id }));
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                // Create notification
                await _notificationService.CreateByReportId(newReport.id, newReport.is_public, departmentIds, divisionIds, groupIds, userIds);
            }
            catch (Exception ex)
            {
                _log.Error($"Create report failed: {ex}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Update(long id, UpdateReportDto request)
        {
            var departmentIds = new List<long>();
            var divisionIds = new List<long>();
            var groupIds = new List<long>();
            var userIds = new List<long>();
            // Parse string to list long
            if (!String.IsNullOrEmpty(request.department_ids))
            {
                departmentIds = ParseIds(request.department_ids ?? String.Empty);
            }
            if (!String.IsNullOrEmpty(request.division_ids))
            {
                divisionIds = ParseIds(request.division_ids ?? String.Empty);
            }
            if (!String.IsNullOrEmpty(request.group_ids))
            {
                groupIds = ParseIds(request.group_ids ?? String.Empty);
            }
            if (!String.IsNullOrEmpty(request.user_ids))
            {
                userIds = ParseIds(request.user_ids ?? String.Empty);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get report
                var report = await _context.reports.Where(r => r.id == id).FirstOrDefaultAsync();

                if (report == null)
                {
                    throw new Exception("Report not found");
                }

                _mapper.Map(request, report);
                report.updated_at = DateTime.Now;

                _context.reports.Update(report);
                await _context.SaveChangesAsync();
                // Delete old relations
                var oldDepartments = _context.report_departments.Where(rd => rd.report_id == id);
                _context.report_departments.RemoveRange(oldDepartments);

                var oldDivisions = _context.report_divisions.Where(rv => rv.report_id == id);
                _context.report_divisions.RemoveRange(oldDivisions);

                var oldGroups = _context.report_groups.Where(rg => rg.report_id == id);
                _context.report_groups.RemoveRange(oldGroups);

                var oldUsers = _context.report_users.Where(ru => ru.report_id == id);
                _context.report_users.RemoveRange(oldUsers);

                await _context.SaveChangesAsync();
                // Create new relations
                if (departmentIds.Any())
                {
                    _context.report_departments.AddRange(departmentIds.Select(id => new ReportDepartment { report_id = report.id, department_id = id }));
                }

                if (divisionIds.Any())
                {
                    _context.report_divisions.AddRange(divisionIds.Select(id => new ReportDivision { report_id = report.id, division_id = id }));
                }

                if (groupIds.Any())
                {
                    _context.report_groups.AddRange(groupIds.Select(id => new ReportGroup { report_id = report.id, group_id = id }));
                }

                if (userIds.Any())
                {
                    _context.report_users.AddRange(userIds.Select(id => new ReportUser { report_id = report.id, user_id = id }));
                }

                // Save change and commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.Error($"Update User Error: {ex.Message}");
                throw;
            }
        }

        public async Task Delete(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get report
                var report = await _context.reports.FirstOrDefaultAsync(r => r.id == id);
                if (report == null)
                {
                    throw new Exception("Report not found");
                }

                // Delete relations
                var departments = _context.report_departments.Where(rd => rd.report_id == id);
                _context.report_departments.RemoveRange(departments);

                var divisions = _context.report_divisions.Where(rv => rv.report_id == id);
                _context.report_divisions.RemoveRange(divisions);

                var groups = _context.report_groups.Where(rg => rg.report_id == id);
                _context.report_groups.RemoveRange(groups);

                var users = _context.report_users.Where(ru => ru.report_id == id);
                _context.report_users.RemoveRange(users);

                await _context.SaveChangesAsync();
                // Remove notification with report id
                await _notificationService.DeleteByReportId(report.id);
                // Delete report
                _context.reports.Remove(report);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _log.Error($"Delete report failed: {ex}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ReportDto?> FindById(long id)
        {
            var report = await _context.reports
                .Where(r => r.id == id)
                .Select(r => new ReportDto
                {
                    id = r.id,
                    title = r.title,
                    content = r.content,
                    date = r.date,
                    is_public = r.is_public,
                    report_type_id = r.report_type_id,
                    report_type_name = r.ReportType!.name,
                    user_id = r.user_id,
                    user_fullname = $"{r.User!.last_name} {r.User!.first_name}",
                    created_at = r.created_at ?? DateTime.MinValue,
                    deleted_at = r.deleted_at,
                    departments = r.ReportDepartment!.Select(rd => new ReportDepartmentDto
                    {
                        report_id = rd.report_id,
                        department_id = rd.department_id,
                        department_name = rd.Department!.name
                    }).ToList(),
                    divisions = r.ReportDivision!.Select(rd => new ReportDivisionDto
                    {
                        report_id = rd.report_id,
                        division_id = rd.division_id,
                        division_name = rd.Division!.name
                    }).ToList(),
                    groups = r.ReportGroup!.Select(rg => new ReportGroupDto
                    {
                        report_id = rg.report_id,
                        group_id = rg.group_id,
                        group_name = rg.Group!.name
                    }).ToList(),
                    users = r.ReportUser!.Select(ru => new ReportUserDto
                    {
                        report_id = ru.report_id,
                        user_id = ru.user_id,
                        user_fullname = $"{ru.User!.last_name} {ru.User!.first_name}"
                    }).ToList(),
                })
                .FirstOrDefaultAsync();

            return report;
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

        public async Task<bool> HasPermisstion(long id)
        {
            long userId = _authService.GetAccountId("Id");
            long roleId = _authService.GetAccountId("Role");
            long departmentId = _authService.GetAccountId("Department");
            long divisionId = _authService.GetAccountId("Division");
            long groupId = _authService.GetAccountId("Group");

            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var report = await _context.reports.Where(r => r.id == id)
                .Include(r => r.ReportDepartment)
                .Include(r => r.ReportDivision)
                .Include(r => r.ReportGroup)
                .Include(r => r.ReportUser)
                .FirstOrDefaultAsync();

            if (report != null)
            {
                if (roleId != (long)RoleEnum.MEMBER)
                {
                    if (roleId == (long)RoleEnum.SYSTEM_ADMIN || report.is_public == true)
                    {
                        return true;
                    } 
                    else
                    {
                        if (managerUserIds.Contains(report.user_id))
                        {
                            return true;
                        }

                        if (report.ReportDepartment?.Any(d => d.department_id == departmentId) == true)
                        {
                            return true;
                        }

                        if (report.ReportDivision?.Any(d => d.division_id == divisionId) == true)
                        {
                            return true;
                        }

                        if (report.ReportGroup?.Any(g => g.group_id == groupId) == true)
                        {
                            return true;
                        }

                        if (report.ReportUser?.Any(u => managerUserIds.Contains(u.user_id)) == true)
                        {
                            return true;
                        }
                    }
                } 
                else
                {
                    if (userId == report.user_id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<bool> HasPermisstionView(long id)
        {
            long userId = _authService.GetAccountId("Id");
            long roleId = _authService.GetAccountId("Role");
            long departmentId = _authService.GetAccountId("Department");
            long divisionId = _authService.GetAccountId("Division");
            long groupId = _authService.GetAccountId("Group");

            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var report = await _context.reports.Where(r => r.id == id)
                .Include(r => r.ReportDepartment)
                .Include(r => r.ReportDivision)
                .Include(r => r.ReportGroup)
                .Include(r => r.ReportUser)
                .FirstOrDefaultAsync();

            if (report != null)
            {
                if (roleId == (long)RoleEnum.SYSTEM_ADMIN || report.is_public == true)
                {
                    return true;
                }
                else
                {
                    if (managerUserIds.Contains(report.user_id))
                    {
                        return true;
                    }

                    if (report.ReportDepartment?.Any(d => d.department_id == departmentId) == true)
                    {
                        return true;
                    }

                    if (report.ReportDivision?.Any(d => d.division_id == divisionId) == true)
                    {
                        return true;
                    }

                    if (report.ReportGroup?.Any(g => g.group_id == groupId) == true)
                    {
                        return true;
                    }

                    if (report.ReportUser?.Any(u => managerUserIds.Contains(u.user_id)) == true)
                    {
                        return true;
                    }

                    if (report.ReportDivision?.Any() == true)
                    {
                        var divisionIds = report.ReportDivision.Select(d => d.division_id).ToList();

                        var groupIdsInDivisions = await _context.groups
                            .Where(g => g.division_id.HasValue && divisionIds.Contains(g.division_id.Value))
                            .Select(g => g.id)
                            .ToListAsync();

                        if (groupIdsInDivisions.Contains(groupId))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public async Task<string> RequestReportDownloadAsync(long reportId)
        {
            // 1. Kiểm tra quyền xem report
            if (!await HasPermisstionView(reportId))
            {
                throw new Exception("Bạn không có quyền tải báo cáo này");
            }

            // 2. Lấy dữ liệu report
            var reportDto = await FindById(reportId);
            if (reportDto == null)
            {
                throw new Exception("Report không tồn tại");
            }

            // 3. Tạo session_id và report_id mới (cho worker xử lý)
            var sessionId = Guid.NewGuid().ToString("N"); // hoặc "session-" + Guid.NewGuid().ToString("N")
            var workerReportId = "report-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);

            // 4. Chuẩn bị message cho SQS
            var message = new
            {
                report_id = workerReportId,
                session_id = sessionId,
                content = reportDto.content ?? "",
                title = reportDto.title,
                is_public = reportDto.is_public,
                report_type_name = reportDto.report_type_name,
                user_fullname = reportDto.user_fullname,
                date = reportDto.date.ToString("yyyy-MM-ddTHH:mm:ss"),
                departments = reportDto.departments,
                divisions = reportDto.divisions,
                groups = reportDto.groups,
                users = reportDto.users
            };

            var messageJson = JsonSerializer.Serialize(message);

            // 5. Gửi message vào SQS
            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _sqsQueueUrl,
                MessageBody = messageJson
            });

            // 6. Lưu trạng thái ban đầu vào DynamoDB
            await _dynamoDbClient.PutItemAsync(new PutItemRequest
            {
                TableName = _dynamoTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "session_id", new AttributeValue { S = sessionId } },
                    { "status", new AttributeValue { S = "pending" } },
                    { "url", new AttributeValue { S = null } },
                    { "created_at", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "report_id", new AttributeValue { S = reportId.ToString() } } // optional
                }
            });

            _log.Info($"Request download report {reportId} - session {sessionId} đã gửi SQS và lưu DynamoDB");

            // 7. Trả session_id về frontend để poll
            return sessionId;
        }

        public async Task<(string Status, string? Url, string? UpdatedAt)> GetReportDownloadStatusAsync(string sessionId)
        {
            var response = await _dynamoDbClient.GetItemAsync(new GetItemRequest
            {
                TableName = _dynamoTableName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "session_id", new AttributeValue { S = sessionId } }
        }
            });

            if (response.Item == null || !response.Item.Any())
                return ("not_found", null, null);

            var item = response.Item;
            var status = item.ContainsKey("status") ? item["status"].S : "unknown";
            var url = item.ContainsKey("url") ? item["url"].S : null;
            var updatedAt = item.ContainsKey("updated_at") ? item["updated_at"].S : null;

            return (status, url, updatedAt);
        }
    }

}
