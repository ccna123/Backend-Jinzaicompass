namespace SystemBrightSpotBE.Dtos.Plan
{
    public class ListPlanDto
    {
        public long id { get; set; }
        public string name { get; set; } = String.Empty;
        public long department_id { get; set; }
        public long? division_id { get; set; }
        public long? group_id { get; set; }
        public long status { get; set; }
        public DateTime? created_at { get; set; }
    }
}
