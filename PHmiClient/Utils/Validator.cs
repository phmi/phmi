using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace PHmiClient.Utils
{
    public static class Validator
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<IDataErrorInfo, object>>> PropertyGetters
            = new Dictionary<Type, Dictionary<string, Func<IDataErrorInfo, object>>>();

        private static readonly Dictionary<Type, Dictionary<string, ValidationAttribute[]>> Validators
            = new Dictionary<Type, Dictionary<string, ValidationAttribute[]>>();

        private static ValidationAttribute[] GetValidationAttrs(ICustomAttributeProvider provider)
        {
            return (ValidationAttribute[])provider
                .GetCustomAttributes(typeof(ValidationAttribute), true);
        }

        private static Func<IDataErrorInfo, object> GetValueGetter(PropertyInfo property)
        {
            return viewmodel => property.GetValue(viewmodel, null);
        }

        private static IEnumerable<Type> GetMetadataTypes(Type type)
        {
            return ((MetadataTypeAttribute[])type
                .GetCustomAttributes(typeof(MetadataTypeAttribute), true))
                .Select(a => a.MetadataClassType);
        }

        private static IEnumerable<PropertyInfo> GetMetadataTypeProperties(Type type)
        {
            var metProps = GetMetadataTypes(type)
                .SelectMany(t => t.GetProperties())
                .Where(p => GetValidationAttrs(p).Any())
                .ToArray();
            return metProps;
        }

        private static Dictionary<string, Func<IDataErrorInfo, object>> GetPropertyGetters(Type type)
        {
            Dictionary<string, Func<IDataErrorInfo, object>> propertyGetters;
            if (PropertyGetters.TryGetValue(type, out propertyGetters))
                return propertyGetters;
            var metProps = GetMetadataTypeProperties(type);
            propertyGetters = type
                .GetProperties()
                .Where(p => GetValidationAttrs(p).Any() || metProps.Any(mp => mp.Name == p.Name))
                .ToDictionary(p => p.Name, GetValueGetter);
            PropertyGetters.Add(type, propertyGetters);
            return propertyGetters;
        }

        private static Dictionary<string, ValidationAttribute[]> GetValidators(Type type)
        {
            Dictionary<string, ValidationAttribute[]> validators;
            if (Validators.TryGetValue(type, out validators))
                return validators;

            var props = type.GetProperties();
            var metProps = GetMetadataTypeProperties(type);
            validators = props
                .Where(p => GetValidationAttrs(p).Any())
                .Concat(metProps.Where(mp => props.Any(p => p.Name == mp.Name)))
                .GroupBy(p => p.Name)
                .ToDictionary(g => g.Key, g => g.SelectMany(GetValidationAttrs).ToArray());
            var attrs = GetValidationAttrs(type)
                .Concat(GetMetadataTypes(type).SelectMany(GetValidationAttrs))
                .ToArray();
            if (attrs.Any())
                validators.Add(string.Empty, attrs);
            Validators.Add(type, validators);
            return validators;
        }

        public static IEnumerable<string> GetErrorMessages(ValidationAttribute[] attributes,
            object value, ValidationContext context)
        {
            var errorMessages = attributes
                    .Select(v => v.GetValidationResult(value, context))
                    .Where(r => r != null)
                    .Select(r => r.ErrorMessage)
                    .Where(e => !string.IsNullOrEmpty(e));
            return errorMessages;
        }

        public static string GetError(this IDataErrorInfo obj, string propertyName)
        {
            var type = obj.GetType();
            var propertyGetters = GetPropertyGetters(type);
            var validators = GetValidators(type);
            if (validators.ContainsKey(propertyName) && propertyGetters.ContainsKey(propertyName))
            {
                var context = new ValidationContext(obj, null, null)
                {
                    MemberName = propertyName,
                    DisplayName = ReflectionHelper.GetDisplayName(obj, propertyName)
                };
                var propertyValue = propertyGetters[propertyName](obj);
                var errorMessages = GetErrorMessages(validators[propertyName], propertyValue, context);
                return string.Join(Environment.NewLine, errorMessages);
            }

            return string.Empty;
        }

        public static string GetError(this IDataErrorInfo obj)
        {
            var type = obj.GetType();
            var propertyGetters = GetPropertyGetters(type);
            var errors = propertyGetters
                .Keys.Select(k => GetError(obj, k))
                .Where(e => !string.IsNullOrEmpty(e));
            var validators = GetValidators(type);
            if (validators.ContainsKey(string.Empty))
            {
                errors = errors.Concat(
                    GetErrorMessages(validators[string.Empty], obj, new ValidationContext(obj, null, null)));
            }
            return string.Join(Environment.NewLine, errors);
        }
    }
}
