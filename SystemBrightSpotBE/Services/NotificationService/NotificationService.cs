using AutoMapper;
using log4net;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Notification;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;

namespace SystemBrightSpotBE.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly DataContext _context;
        private readonly ILog _log;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        public NotificationService(
            DataContext context,
            IMapper mapper,
            IAuthService authService
        )
        {
            _context = context;
            _mapper = mapper;
            _log = LogManager.GetLogger(typeof(NotificationService));
            _authService = authService;
        }

        public async Task CreateByReportId(long reportId, bool? isPublic, List<long> departmentIds, List<long> divisionIds, List<long> groupIds, List<long> userIds)
        {
            var nameCreator = _authService.GetAccountFullName();
            long tenantId = _authService.GetAccountId("Tenant");

            var dataQuery = _context.users.Where(u => u.deleted_at == null).Where(u => u.tenant_id == tenantId).AsQueryable();

            if (isPublic == false)
            {
                List<long> groupIdsInDivisions = new();
                if (divisionIds != null && divisionIds.Any())
                {
                    groupIdsInDivisions = await _context.groups
                        .Where(g => g.division_id.HasValue && divisionIds.Contains(g.division_id.Value))
                        .Select(g => g.id)
                        .ToListAsync();
                }

                var allGroupIds = new List<long>();
                if (groupIds != null && groupIds.Any())
                {
                    allGroupIds.AddRange(groupIds);
                }
                   
                if (groupIdsInDivisions.Any())
                {
                    allGroupIds.AddRange(groupIdsInDivisions);
                }
                

                dataQuery = dataQuery.Where(u =>
                    (departmentIds.Any() && u.department_id.HasValue && departmentIds.Contains(u.department_id.Value)) ||
                    (divisionIds!.Any() && u.division_id.HasValue && divisionIds!.Contains(u.division_id.Value)) ||
                    (allGroupIds.Any() && u.group_id.HasValue && allGroupIds.Contains(u.group_id.Value)) ||
                    (userIds.Any() && userIds.Contains(u.id))
                );
            }

            List<long> uIds = await dataQuery.Where(u => u.tenant_id == tenantId).Select(u => u.id).Distinct().ToListAsync();
            // Create notification with list user
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var content = $"「{nameCreator}」はレポートを作成しました。この通知をクリックして確認してください。";

                foreach (long uid in uIds)
                {
                    _context.notifications.Add(new Notification
                    {
                        content = content,
                        is_read = false,
                        report_id = reportId,
                        user_id = uid
                    });
                }
                // Save change and commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _log.Error($"Create notification failed: {ex}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteByReportId(long reportId)
        {
            var notifications = await _context.notifications
                .Where(n => n.report_id == reportId)
                .ToListAsync();

            if (notifications.Any())
            {
                _context.notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<NotificationDto>> GetAll()
        {
            long userId = _authService.GetAccountId("Id");
            var monthAgo = DateTime.Now.AddMonths(-1);

            var notifications = await _context.notifications
                .Where(n => n.user_id == userId && n.created_at >= monthAgo)
                .OrderByDescending(n => n.created_at)
                .Select(n => new NotificationDto {
                    title = n.Report!.title,
                    content = n.content,
                    is_read = n.is_read,
                    report_id = n.report_id,
                    user_id = n.user_id,
                    created_at = n.created_at
                })
                .ToListAsync();

            return notifications;
        }

        public async Task ReadByReportId(long reportId)
        {
            long userId = _authService.GetAccountId("Id");

            var notification = await _context.notifications
                .Where(n => n.report_id == reportId && n.user_id == userId)
                .FirstOrDefaultAsync();

            if (notification != null)
            {
                if (!notification.is_read)
                {
                    notification.is_read = true;
                    _context.notifications.Update(notification);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<NotificationDto?> FindByReportId(long reportId)
        {
            long userId = _authService.GetAccountId("Id");

            var notification = await _context.notifications
                .Where(n => n.report_id == reportId && n.user_id == userId)
                .Select(n => new NotificationDto
                {
                    content = n.content,
                    is_read = n.is_read,
                    report_id = n.report_id,
                    user_id = n.user_id,
                    created_at = n.created_at
                })
                .FirstOrDefaultAsync();

            return notification;
        }
    }
}
