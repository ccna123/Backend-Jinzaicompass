namespace SystemBrightSpotBE.Models
{
    public class ExperienceJob : BaseCategoryEntityModel
    {
        public ICollection<ExperienceField> ExperienceFields { get; set; } = new List<ExperienceField>();
    }
}
