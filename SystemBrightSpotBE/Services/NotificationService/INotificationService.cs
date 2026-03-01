using SystemBrightSpotBE.Dtos.Notification;

namespace SystemBrightSpotBE.Services.NotificationService
{
    public interface INotificationService
    {
        Task CreateByReportId(long reportId, bool? isPublic, List<long> departments, List<long> divisions, List<long> groups, List<long> users);
        Task<List<NotificationDto>> GetAll();
        Task ReadByReportId(long reportId);
        Task DeleteByReportId(long reportId);
        Task<NotificationDto?> FindByReportId(long reportId);
    }
}
