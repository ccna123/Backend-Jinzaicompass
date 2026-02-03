using AutoMapper;
using log4net;
using Microsoft.EntityFrameworkCore;
using SystemBrightSpotBE.Dtos.Setting;
using SystemBrightSpotBE.Models;
using SystemBrightSpotBE.Services.AuthService;
using SystemBrightSpotBE.Services.S3Service;

namespace SystemBrightSpotBE.Services.SettingService
{
    public class SettingService : ISettingService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        private readonly IAuthService _authService;
        private readonly IS3Service _s3Service;

        public SettingService(
           DataContext context,
           IMapper mapper,
           IAuthService authService,
           IS3Service s3Service
        )
        {
            _log = LogManager.GetLogger(typeof(SettingService));
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _s3Service = s3Service;
        }

        public async Task<SettingDto> GetSetting()
        {
            long tenantId = _authService.GetAccountId("Tenant");
            var setting = await _context.settings.Where(s => s.tenant_id == tenantId).FirstOrDefaultAsync();

            return _mapper.Map<SettingDto>(setting);
        }

        public async Task UpdateSetting(UpdateSettingDto request, bool removeFile)
        {
            long tenantId = _authService.GetAccountId("Tenant");
            long userId = _authService.GetAccountId("Id");

            var setting = await _context.settings.Where(s => s.tenant_id == tenantId).FirstOrDefaultAsync();

            if (setting != null)
            {
                // Update setting
                setting.name = request.name;
                // Check file upload
                if (request.file != null && request.file.Length > 0)
                {
                    setting.file_name = request.file.FileName;
                    var resultUpload = await _s3Service.UploadFileAsync(request.file, folder: "setting", width: 150);
                    if (resultUpload != null)
                    {
                        setting.file_url = resultUpload.ToString();
                    }

                    var resultUploadThumb = await _s3Service.UploadFileAsync(request.file, folder: "setting", width: 36);
                    if (resultUploadThumb != null)
                    {
                        setting.file_url_thumb = resultUploadThumb.ToString();
                    }
                } 
                else
                {
                    if (removeFile)
                    {
                        if (!String.IsNullOrEmpty(setting.file_url))
                        {
                            await _s3Service.DeleteFileAsync(setting.file_url);
                        }
                        setting.file_name = String.Empty;
                        setting.file_url = String.Empty;
                        setting.file_url_thumb = String.Empty;
                    }
                }
                setting.user_id = userId;
                setting.updated_at = DateTime.Now;
            } 
            else
            {
                // Create setting
                var newSetting = new Setting
                {
                    name = request.name,
                    user_id = userId,
                    file_name = String.Empty,
                    file_url = String.Empty,
                    file_url_thumb = String.Empty,
                    tenant_id = tenantId,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                };
                // Check file upload
                if (request.file != null && request.file.Length > 0)
                {
                    newSetting.file_name = request.file.FileName;

                    var resultUpload = await _s3Service.UploadFileAsync(request.file, folder: "setting", width: 150);
                    if (resultUpload != null)
                    {
                        newSetting.file_url = resultUpload.ToString();
                    }

                    var resultUploadThumb = await _s3Service.UploadFileAsync(request.file, folder: "setting", width: 36);
                    if (resultUploadThumb != null)
                    {
                        newSetting.file_url_thumb = resultUploadThumb.ToString();
                    }
                }

                await _context.settings.AddAsync(newSetting);
            }
            // Save change
            await _context.SaveChangesAsync();
        }
    }
}
