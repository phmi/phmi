using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace PHmiClient.Controls
{
    public class DataErrorInfoValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var bindingGroup = (BindingGroup)value;
            if (bindingGroup != null)
            {
                var course = bindingGroup.Items[0] as IDataErrorInfo;
                if (course != null)
                {
                    if (!string.IsNullOrEmpty(course.Error))
                    {
                        return new ValidationResult(false, course.Error);
                    }
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}
