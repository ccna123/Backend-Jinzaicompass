using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Dashboard;
using SystemBrightSpotBE.Dtos.Report;
using SystemBrightSpotBE.Enums;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.UserService;

namespace SystemBrightSpotBE.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public DashboardService(
            DataContext context,
            IMapper mapper,
            IAuthService authService,
            IUserService userService
        )
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _userService = userService;
        }

        public async Task<List<UserByMonthDto>> CalculateUserByMonth()
        {
            var ids = await _userService.GetManagedUsersId();
            var today = DateTime.Today;
            var minDate = new DateOnly(today.Year, today.Month, 1).AddMonths(-11);
            var result = new List<UserByMonthDto>();

            for (int i = 0; i < 12; i++)
            {
                var monthDate = today.AddMonths(-i);
                var startOfMonth = new DateOnly(monthDate.Year, monthDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var history = await _context.user_status_history
                    .Where(h => ids.Contains(h.user_id) && (h.date >= minDate && h.date <= endOfMonth))
                    .ToListAsync();

                var latestStatuses = history
                    .GroupBy(h => h.user_id)
                    .Select(g => g.OrderByDescending(x => x.date).First())
                    .Where(x => x.type != (int)StatusHistoryTypeEnum.Resign)
                    .ToList();

                int count = latestStatuses.Count;

                result.Add(new UserByMonthDto
                {
                    month = $"{startOfMonth:yyyy-MM}",
                    count = count
                });
            }

            return result;
        }

        public async Task<List<UserByYearDto>> CalculateUserByYear()
        {
            var ids = await _userService.GetManagedUsersId();
            var today = DateTime.Today;
            var currentYear = today.Year;
            var result = new List<UserByYearDto>();

            // 5 years
            for (int i = 0; i < 5; i++)
            {
                var year = currentYear - i;
                var startOfYear = new DateOnly(year, 1, 1);
                var endOfYear = new DateOnly(year, 12, 31);

                var history = await _context.user_status_history
                    .Where(h => ids.Contains(h.user_id) && h.date <= endOfYear)
                    .ToListAsync();

                var latestStatuses = history
                    .GroupBy(h => h.user_id)
                    .Select(g => g.OrderByDescending(x => x.date).First())
                    .Where(x => x.type != (int)StatusHistoryTypeEnum.Resign)
                    .ToList();

                int count = latestStatuses.Count;

                result.Add(new UserByYearDto
                {
                    year = year.ToString(),
                    count = count
                });
            }

            return result;
        }

        public async Task<List<UserRecentDto>> GetUserRecent()
        {
            var ids = await _userService.GetManagedUsersId();
            var today = DateOnly.FromDateTime(DateTime.Today);

            var recentUsers = await _context.users
                .Where(u => ids.Contains(u.id))
                .Select(u => new
                {
                    full_name = $"{u.last_name} {u.first_name}",
                    gender_name = u.Gender!.name,
                    department_name = u.Department!.name,
                    LatestEntryDate = u.UserStatusHistory!
                        .Where(h => h.type == (int)StatusHistoryTypeEnum.Join)
                        .OrderBy(h => h.date)
                        .Select(h => (DateOnly?)h.date)
                        .FirstOrDefault()
                })
                .Where(x => x.LatestEntryDate != null && x.LatestEntryDate < today)
                .OrderByDescending(x => x.LatestEntryDate)
                .Take(3)
                .Select(x => new UserRecentDto
                {
                    full_name = x.full_name,
                    gender_name = x.gender_name,
                    department_name = x.department_name,
                    date_joining_company = x.LatestEntryDate!.Value
                })
                .ToListAsync();

            return recentUsers;
        }

        public async Task<List<ReportRecentDto>> GetReportRecent()
        {
            long userId = _authService.GetAccountId("Id");
            long departmentId = _authService.GetAccountId("Department");
            long divisionId = _authService.GetAccountId("Division");
            long groupId = _authService.GetAccountId("Group");
            long roleId = _authService.GetAccountId("Role");
            long tenantId = _authService.GetAccountId("Tenant");

            List<long> managerUserIds = await _userService.GetManagedUsersId();
            // Where by permission
            var dataQuery = _context.reports.Where(r => r.tenant_id == tenantId).Select(r => new
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
                }).AsQueryable();

            switch (roleId)
            {
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

            return await dataQuery
               .Select(r => new ReportRecentDto
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
               })
               .OrderByDescending(r => r.date).ThenByDescending(r => r.created_at)
               .Skip(0)
               .Take(3)
               .ToListAsync();
        }

        public async Task<List<RatioDto>> CalculateParticipationPositionRatio()
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var today = DateOnly.FromDateTime(DateTime.Today);
            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var result = await _context.participation_positions
                .Where(p => p.tenant_id == tenantId)
                .Select(pos => new RatioDto
                {
                    name = pos.name,
                    count = _context.project_participation_position.Count(ppp => 
                        ppp.participation_position_id == pos.id && 
                        ppp.Project != null && 
                        ppp.Project.start_date < today &&
                        managerUserIds.Contains(ppp.Project!.user_id!.Value)
                    )
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<UserSeniorityDto>> CalculateUserSeniority()
        {
            var ids = await _userService.GetManagedUsersId();
            var today = DateTime.Today;
            var now = DateOnly.FromDateTime(DateTime.Today);

            var result = await _context.users
                .Where(u => ids.Contains(u.id))
                .Select(u => new
                {
                    JoinDate = u.UserStatusHistory!
                        .Where(s => s.type == (int)StatusHistoryTypeEnum.Join)
                        .OrderBy(s => s.date)
                        .Select(s => (DateTime?)s.date.ToDateTime(TimeOnly.MinValue))
                        .FirstOrDefault(),

                    LatestStatus = u.UserStatusHistory!
                        .Where(s => s.date <= now)
                        .OrderByDescending(s => s.date)
                        .Select(s => (int?)s.type)
                        .FirstOrDefault()
                })
                .ToListAsync();


            var grouped = result
                .Where(x => x.JoinDate.HasValue && x.LatestStatus != (int)StatusHistoryTypeEnum.Resign)
                .GroupBy(x =>
                {
                    var yearDiff = (today - x.JoinDate!.Value).TotalDays / 365;
                    if (yearDiff < 2)
                    {
                        return "0～1年";
                    }
                    else if (yearDiff <= 3)
                    {
                        return "2～3年";
                    }
                    else if (yearDiff <= 5)
                    {
                        return "4～5年";
                    }
                    else if (yearDiff <= 7)
                    {
                        return "6～7年";
                    }
                    else if (yearDiff <= 9)
                    {
                        return "8～9年";
                    }
                    else if (yearDiff <= 11)
                    {
                        return "10～11年";
                    }
                    else if (yearDiff <= 15)
                    {
                        return "12～15年";
                    }
                    else if (yearDiff <= 20)
                    {
                        return "16～20年";
                    }
                    else
                    {
                        return "21～年";
                    }
                })
                .Select(g => new UserSeniorityDto
                {
                    range = g.Key,
                    count = g.Count()
                })
                .ToDictionary(x => x.range, x => x.count);

            var ranges = new[] { 
                "0～1年", "2～3年", "4～5年", "6～7年",
                "8～9年", "10～11年", "12～15年",
                "16～20年", "21～年" 
            };

            var finalResult = ranges
                .Select(r => new UserSeniorityDto
                {
                    range = r,
                    count = grouped.ContainsKey(r) ? grouped[r] : 0
                })
                .ToList();

            return finalResult;
        }

        public async Task<List<RatioDto>> CalculateExperienceJobRatio()
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var today = DateOnly.FromDateTime(DateTime.Today);
            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var result = await _context.experience_jobs
                .Where(job => job.tenant_id == tenantId)
                .Select(job => new RatioDto
                {
                    name = job.name,
                    count = _context.project_experience_job.Count(pej => 
                        pej.experience_job_id == job.id && 
                        pej.Project != null && 
                        pej.Project.start_date < today &&
                        managerUserIds.Contains(pej.Project!.user_id!.Value)
                    )
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<RatioDto>> CalculateExperienceFieldRatio()
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var today = DateOnly.FromDateTime(DateTime.Today);
            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var result = await _context.experience_fields
                .Where(field => field.tenant_id == tenantId)
                .Select(field => new RatioDto
                {
                    name = field.name,
                    count = _context.project_experience_field.Count(pef => 
                        pef.experience_field_id == field.id && 
                        pef.Project != null && 
                        pef.Project.start_date < today &&
                        managerUserIds.Contains(pef.Project!.user_id!.Value)
                    )
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<RatioDto>> CalculateExperienceAreaRatio()
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var today = DateOnly.FromDateTime(DateTime.Today);
            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var result = await _context.experience_areas
                .Where(area => area.tenant_id == tenantId)
                .Select(area => new RatioDto
                {
                    name = area.name,
                    count = _context.project_experience_area.Count(pea => 
                        pea.experience_area_id == area.id && 
                        pea.Project != null && 
                        pea.Project.start_date < today &&
                        managerUserIds.Contains(pea.Project!.user_id!.Value)
                    )
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<RatioDto>> CalculateSpecificSkillRatio()
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var today = DateOnly.FromDateTime(DateTime.Today);
            List<long> managerUserIds = await _userService.GetManagedUsersId();

            var result = await _context.specific_skills
                .Where(skill => skill.tenant_id == tenantId)
                .Select(skill => new RatioDto
                {
                    name = skill.name,
                    count = _context.project_specific_skill.Count(pss => 
                        pss.specific_skill_id == skill.id && 
                        pss.Project != null && 
                        pss.Project.start_date < today &&
                        managerUserIds.Contains(pss.Project!.user_id!.Value)
                    )
                })
                .ToListAsync();

            return result;
        }
    }
}
