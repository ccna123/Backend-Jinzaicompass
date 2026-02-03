namespace SystemBrightSpotBE.Dtos.Notification
{
    public class NotificationDto
    {
        public string title { get; set; } = String.Empty;
        public string content { get; set; } = String.Empty;
        public bool is_read { get; set; }
        public long report_id { get; set; }
        public long user_id { get; set; }
        public DateTime? created_at { get; set; }
    }
}
