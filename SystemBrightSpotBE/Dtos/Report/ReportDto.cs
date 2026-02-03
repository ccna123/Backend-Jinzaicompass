using SystemBrightSpotBE.Dtos.Report;

public class ReportDto
{
    public long id {  get; set; }
    public string title { get; set; } = String.Empty;
    public string content { get; set; } = String.Empty;
    public DateOnly date { get; set; }
    public long report_type_id { get; set; }
    public string report_type_name { get; set; } = String.Empty;
    public bool? is_public { get; set; } = true;
    public long user_id { get; set; }
    public string user_fullname { get; set; } = String.Empty;
    public DateTime created_at { get; set; }
    public DateTime? deleted_at { get; set; }
    public List<ReportDepartmentDto> departments { get; set; } = new List<ReportDepartmentDto>();
    public List<ReportDivisionDto> divisions { get; set; } = new List<ReportDivisionDto>();
    public List<ReportGroupDto> groups { get; set; } = new List<ReportGroupDto>();
    public List<ReportUserDto> users { get; set; } = new List<ReportUserDto>();
}
