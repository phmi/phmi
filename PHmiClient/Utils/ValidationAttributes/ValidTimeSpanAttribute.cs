using System;
using System.ComponentModel.DataAnnotations;

namespace PHmiClient.Utils.ValidationAttributes
{
    public class ValidTimeSpanAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                if (AllowNull)
                    return ValidationResult.Success;
            }
            TimeSpan t;
            if (TimeSpan.TryParse(str, out t))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(
                string.Format(ErrorMessageString, validationContext.DisplayName, new TimeSpan(11, 22, 33, 44, 55)),
                new[] { validationContext.MemberName });
        }

        public bool AllowNull { get; set; }
    }
}
