
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemBrightSpotBE.Models
{
    public class Group : BaseCategoryEntityModel
    {
        [Required]
        [ForeignKey("Department")]
        [DefaultValue(1)]
        public long department_id { get; set; } = 1;
        public Department? Department { get; set; }

        [ForeignKey("Division")]
        public long? division_id { get; set; }
        public Division? Division { get; set; }
    }
}
