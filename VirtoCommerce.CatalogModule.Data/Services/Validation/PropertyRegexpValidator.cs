using System.Collections.Generic;
using System.Text.RegularExpressions;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class PropertyRegexpValidator : IPropertyValueValidator
    {
        private IPropertyValueValidator _next;

        public PropertyRegexpValidator(IPropertyValueValidator validator)
        {
            _next = validator;
        }

        public string[] Validate(PropertyValidationRule rule, PropertyValue propertyValue)
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(rule.RegExp))
            {
                var regexp = new Regex(rule.RegExp, RegexOptions.IgnoreCase);
                var match = regexp.Match(propertyValue.Value.ToString());
                if (!match.Success)
                {
                    errors.Add("propertyRegexpValidatorError");
                }
            }

            if (_next != null)
                errors.AddRange(_next.Validate(rule, propertyValue));

            return errors.ToArray();
        }
    }
}