using SystemBrightSpotBE.Base.Pagination;
using SystemBrightSpotBE.Dtos.Report;

namespace SystemBrightSpotBE.Services.ReportService
{
    public interface IReportService
    {
        Task<PagedResponse<List<ReportDto>>> Search(ListReportParamDto request);
        Task Create(CreateReportDto request);
        Task Update(long id, UpdateReportDto request);
        Task Delete(long id);
        Task<ReportDto?> FindById(long id);
        Task<bool> HasPermisstion(long id);
        Task<bool> HasPermisstionView(long id);
        Task<ReportPDFDto> DowloadPDF(long id);
        Task<byte[]> ConvertHtmlToPDF(ReportPDFDto data);
    }
}
