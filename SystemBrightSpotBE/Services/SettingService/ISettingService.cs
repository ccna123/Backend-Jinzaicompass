using SystemBrightSpotBE.Dtos.Setting;

namespace SystemBrightSpotBE.Services.SettingService
{
    public interface ISettingService
    {
        Task<SettingDto> GetSetting();
        Task UpdateSetting(UpdateSettingDto request, bool removeFile = false);
    }
}
