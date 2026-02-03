using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Attributes
{
    public class EmailOrAdministratorAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid( object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var email = value.ToString();
            if (email == "Administrator")
            {
                return ValidationResult.Success;
            }

            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(email))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
