using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DisallowEnumValuesAttribute : ValidationAttribute
    {
        private readonly object[] _disallowedValues;

        public DisallowEnumValuesAttribute(params object[] disallowedValues)
        {
            _disallowedValues = disallowedValues ?? Array.Empty<object>();
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (_disallowedValues.Contains(value))
            {
                var disallowedList = string.Join(", ", _disallowedValues.Select(v => v.ToString()));
                return new ValidationResult(
                    $"値「{value}」は許可されていません。禁止されている値: {disallowedList}。"
                );
            }

            return ValidationResult.Success;
        }
    }
}
