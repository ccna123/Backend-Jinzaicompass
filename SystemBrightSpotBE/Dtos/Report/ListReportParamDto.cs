using System.Text.Json.Serialization;

namespace SystemBrightSpotBE.Dtos.Report
{
    public class ListReportParamDto
    {
        public int page { get; set; } = 1;
        public int size { get; set; } = 20;
        public string column { get; set; } = "updated_at";
        public string order { get; set; } = "desc";
        public string? title { get; set; }
        public long? report_type_id { get; set; }
        public string? user_ids { get; set; }
        public long? viewer_id { get; set; }
    }
}
