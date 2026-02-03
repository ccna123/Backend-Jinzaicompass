
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Division : BaseCategoryEntityModel
    {
        [Required]
        [ForeignKey("Department")]
        public long department_id { get; set; }
        public Department? Department { get; set; }
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
