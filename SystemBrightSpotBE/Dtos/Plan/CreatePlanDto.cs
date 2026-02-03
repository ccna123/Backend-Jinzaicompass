using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Dtos.Plan.PlanCondition;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Plan
{
    public class CreatePlanDto : IValidatableObject
    {
        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "NameRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "NameMaxLength")]
        public string name { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "DescriptionRequired")]
        [MaxLength(64, ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "DescriptionMaxLength")]
        public string description { get; set; } = String.Empty;

        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "StartDateRequired")]
        public DateOnly start_date { get; set; }

        [Required(ErrorMessageResourceType = typeof(PlanResource), ErrorMessageResourceName = "CompleteDateRequired")]
        public DateOnly complete_date { get; set; }

        [Required]
        public List<PlanConditionDto> conditions { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (complete_date <= start_date)
            {
                yield return new ValidationResult(
                    PlanResource.CompleteDateMustBeGreaterThanStartDate,
                    new[] { nameof(complete_date) }
                );
            }
        }
    }
}
