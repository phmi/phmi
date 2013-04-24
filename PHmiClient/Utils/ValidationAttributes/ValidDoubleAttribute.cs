using System.ComponentModel.DataAnnotations;

namespace PHmiClient.Utils.ValidationAttributes
{
    public class ValidDoubleAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                if (AllowNull)
                    return ValidationResult.Success;
            }
            else
            {
                double d;
                if (double.TryParse(str, out d))
                {
                    if ((AllowInfinity || !double.IsInfinity(d)) && (AllowNaN || !double.IsNaN(d)))
                        return ValidationResult.Success;
                }
            }
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), new [] { validationContext.MemberName });
        }

        public bool AllowNull { get; set; }

        public bool AllowInfinity { get; set; }

        public bool AllowNaN { get; set; }
    }
}
