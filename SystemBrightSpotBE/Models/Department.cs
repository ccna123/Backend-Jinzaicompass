
namespace SystemBrightSpotBE.Models
{
    public class Department : BaseCategoryEntityModel
    {
        public ICollection<Division> Divisions { get; set; } = new List<Division>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
