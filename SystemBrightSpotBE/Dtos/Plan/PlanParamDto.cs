using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Dtos.Plan
{
    public class PlanParamDto
    {
        public long department_id { get; set; }
        public long division_id { get; set; }
        [Range(0, 3)]
        public int status { get; set; } = 2;
    }
}
