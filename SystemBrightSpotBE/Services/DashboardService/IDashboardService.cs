using SystemBrightSpotBE.Dtos.Dashboard;

namespace SystemBrightSpotBE.Services.DashboardService
{
    public interface IDashboardService
    {
        Task<List<UserByMonthDto>> CalculateUserByMonth();
        Task<List<UserByYearDto>> CalculateUserByYear();
        Task<List<UserRecentDto>> GetUserRecent();
        Task<List<ReportRecentDto>> GetReportRecent();
        Task<List<RatioDto>> CalculateParticipationPositionRatio();
        Task<List<UserSeniorityDto>> CalculateUserSeniority();
        Task<List<RatioDto>> CalculateExperienceJobRatio();
        Task<List<RatioDto>> CalculateExperienceFieldRatio();
        Task<List<RatioDto>> CalculateExperienceAreaRatio();
        Task<List<RatioDto>> CalculateSpecificSkillRatio();
    }
}
