using SystemBrightSpotBE.Dtos.MonitoringSystem;
using SystemBrightSpotBE.Models;

namespace SystemBrightSpotBE.Services.MonitoringSystemService
{
    public interface IMonitoringSystemService
    {
        Task<List<MonitoringSystemDto>> GetAll();
        Task Create(CreateMonitoringSystemDto request);
        Task Delete(long id);
        Task<MonitoringSystemDto> FindById(long id);
        Task<bool> CheckEmailExist(string value, bool update = false, long id = 0);
        bool IsCurrentPasswordValid(User user, string currentPassword);
        Task ChangePassword(User user, MonitoringSystemChangePasswordDto request);
    }
}
