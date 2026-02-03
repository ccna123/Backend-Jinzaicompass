using System.ComponentModel.DataAnnotations;
using SystemBrightSpotBE.Resources;

namespace SystemBrightSpotBE.Dtos.Tenant
{
    public class UpdateExpiredTenantDto
    {
        [Required(ErrorMessageResourceType = typeof(TenantResource), ErrorMessageResourceName = "EndDateRequired")]
        public DateOnly end_date { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (end_date < today)
            {
                yield return new ValidationResult(TenantResource.EndDateMustBeAfterStartDate, new[] { nameof(end_date) });
            }
        }
    }
}
