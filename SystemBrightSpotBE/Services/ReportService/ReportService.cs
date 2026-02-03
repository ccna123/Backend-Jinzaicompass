using AutoMapper;
using HtmlAgilityPack;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
        public ReportService(
            DataContext context,
            IMapper mapper,
            IAuthService authService,
            IUserService userService,
            INotificationService notificationService
        )
        {
            _context = context;
            _mapper = mapper;
            _log = LogManager.GetLogger(typeof(ReportService));
            _authService = authService;
            _userService = userService;
            _notificationService = notificationService;
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

        public async Task<ReportPDFDto> DowloadPDF(long id)
        {
            var report = await FindById(id);
            var reportPDF = new ReportPDFDto();

            if (report != null)
            {
                reportPDF.title = report.title;
                reportPDF.content = report.content;
                reportPDF.date = report.date;
                reportPDF.report_type_name = report.report_type_name;
                reportPDF.user_fullname = report.user_fullname;
                // Convert department, division, group, user
                if (report.is_public == true)
                {
                    reportPDF.target_all = "公開";
                }
                else
                {
                    List<string> targetName = new List<string>();

                    if (report.departments.Any())
                    {
                        targetName.AddRange(report.departments.Select(d => d.department_name));
                    }

                    if (report.divisions.Any())
                    {
                        targetName.AddRange(report.divisions.Select(d => d.division_name));
                    }

                    if (report.groups.Any())
                    {
                        targetName.AddRange(report.groups.Select(g => g.group_name));
                    }

                    if (report.users.Any())
                    {
                        targetName.AddRange(report.users.Select(u => u.user_fullname));
                    }

                    reportPDF.target_all = String.Join("、", targetName);
                }
            }

            return reportPDF;
        }

        public async Task<byte[]> ConvertHtmlToPDF(ReportPDFDto data)
        {
            string html = PageHTML(data);

            using var playwright = await Playwright.CreateAsync();
           
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage", "--disable-gpu" }
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "15mm", Bottom = "15mm", Left = "10mm", Right = "10mm" }
            });

            return pdfBytes;
        }

        private string ParseContentToHTML(string contentHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(contentHtml);


            var nodes = doc.DocumentNode.SelectNodes("//*");
            if (nodes != null)
            {
                var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "p", "pre", "h1", "h2", "h3", "h4", "h5", "h6"
                };

                var unUsedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "iframe", "video", "embed", "script"
                };

                int orderCounter = 0;

                foreach (var node in nodes)
                {
                    // Remove unwanted tags
                    if (unUsedTags.Contains(node.Name))
                    {
                        node.Remove();
                        continue;
                    }

                    var cls = node.GetAttributeValue("class", "");
                    var style = node.GetAttributeValue("style", "");

                    // Default style
                    if (tags.Contains(node.Name))
                    {
                        style += "margin: 0; padding: 0;";
                    }

                    if (node.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        style += "min-height: 18px;";
                    }

                    if (node.Name.Equals("h1", StringComparison.OrdinalIgnoreCase))
                    {
                        style += "font-size: 32px;";
                    }

                    if (node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
                    {
                        style += "font-size: 24px;";
                    }

                    if (node.Name.Equals("li", StringComparison.OrdinalIgnoreCase))
                    {
                        var dataList = node.GetAttributeValue("data-list", "");

                        if (dataList == "bullet")
                        { 
                            style += "list-style-type: disc;";
                        }
                        else if (dataList == "ordered")
                        {
                            orderCounter++;
                            node.SetAttributeValue("value", orderCounter.ToString());
                            style += "list-style-type: decimal; list-style-position: outside;";
                        }

                        if (!string.IsNullOrEmpty(style.Trim()))
                        {
                            node.SetAttributeValue("style", style.Trim());
                        }
                        node.Attributes.Remove("data-list");
                    }

                    // Align
                    if (cls.Contains("ql-align-left"))
                    {
                        style += "text-align: left;";
                    }
                    if (cls.Contains("ql-align-center"))
                    {
                        style += "text-align: center;";
                    }
                    if (cls.Contains("ql-align-right"))
                    {
                        style += "text-align: right;";
                    }
                    if (cls.Contains("ql-align-justify"))
                    {
                        style += "text-align: justify;";
                    }

                    // Size
                    if (cls.Contains("ql-size-small"))
                    {
                        style += "font-size: 12px;";
                    }
                    if (cls.Contains("ql-size-large"))
                    {
                        style += "font-size: 24px;";
                    }
                    if (cls.Contains("ql-size-huge"))
                    {
                        style += "font-size: 40px; font-weight: bold;";
                    }

                    // Padding left
                    if (cls.Contains("ql-indent-1"))
                    {
                        style += "padding-left: 3em;";
                    }
                    if (cls.Contains("ql-indent-2"))
                    {
                        style += "padding-left: 6em;";
                    }
                    if (cls.Contains("ql-indent-3"))
                    {
                        style += "padding-left: 9em;";
                    }
                    if (cls.Contains("ql-indent-4"))
                    {
                        style += "padding-left: 12em;";
                    }
                    if (cls.Contains("ql-indent-5"))
                    {
                        style += "padding-left: 15em;";
                    }
                    if (cls.Contains("ql-indent-6"))
                    {
                        style += "padding-left: 18em;";
                    }
                    if (cls.Contains("ql-indent-7"))
                    {
                        style += "padding-left: 21em;";
                    }
                    if (cls.Contains("ql-indent-8"))
                    {
                        style += "padding-left: 24em;";
                    }

                    // Set style
                    if (!string.IsNullOrEmpty(style.Trim()))
                    {
                        node.SetAttributeValue("style", style.Trim());
                    }

                    if (!string.IsNullOrEmpty(cls))
                    {
                        node.Attributes.Remove("class");
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        private string PageHTML(ReportPDFDto data)
        {
            var parseContent = ParseContentToHTML(data.content);

            var htmlString = $@"
                <!DOCTYPE html>
                <html lang='ja'>
                   <head>
                      <meta charset='UTF-8'>
                      <style>
                         body {{
                             font-family: 'Noto Sans JP', sans-serif;
                             margin: 0;
                             padding: 0;
                             font-size: 12pt;
                         }}
                         .header-table {{
                             width: 100%;
                             border-collapse: collapse;
                             margin-bottom: 10px;
                         }}
                         .header-table th {{
                             background-color: #d9e9f3;
                             text-align: left;
                             padding: 6px 10px;
                             font-weight: normal;
                             white-space: nowrap;
                             width: 80px;
                         }}
                         .header-table td {{
                             padding: 6px 10px;
                             background-color: #F3F3F3;
                         }}
                         .content-title {{
                             margin-top: 30px;
                             padding: 6px 10px;
                             background-color: #D2E1E7;
                         }}
                         .content {{
                             padding: 20px;
                             background-color: #FBFBFB;
                             border: 1px solid #BFBFBF;
                             border-top: none;
                             min-height: 200px;
                         }}
                      </style>
                   </head>
                   <body>
                      <table class='header-table'>
                         <tr>
                            <th>起票者</th>
                            <td>{data.user_fullname}</td>
                            <th>起票日</th>
                            <td>{data.date.ToString("yyyy/MM/dd")}</td>
                            <th>タイプ</th>
                            <td>{data.report_type_name}</td>
                         </tr>
                      </table>
                      <table class='header-table'>
                         <tr>
                            <th>閲覧者</th>
                            <td colspan='5'>{data.target_all}</td>
                         </tr>
                      </table>
                      <div class='content-title'>内容</div>
                      <div class='content'>
                         {parseContent}
                      </div>
                   </body>
                </html>";

            return htmlString;
        }
    }
}
