using System.Collections.Generic;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class PropertyLenghtValidator : IPropertyValueValidator
    {
        private IPropertyValueValidator _next;

        public PropertyLenghtValidator(IPropertyValueValidator validator)
        {
            _next = validator;
        }

        public string[] Validate(PropertyValidationRule rule, PropertyValue propertyValue)
        {
            var errors = new List<string>();

            var value = propertyValue.Value.ToString();

            if ((rule.CharCountMax.HasValue && rule.CharCountMin.HasValue) &&
                (value.Length < rule.CharCountMin || value.Length > rule.CharCountMax))
            {
                return new string[] { "propertyRangeLenghtValidatorError" };
            }

            if ((rule.CharCountMax.HasValue && !rule.CharCountMin.HasValue) &&
                (value.Length > rule.CharCountMax))
            {
                return new string[] { "propertyMaxLenghtValidatorError" };
            }

            if ((!rule.CharCountMax.HasValue && rule.CharCountMin.HasValue) &&
                (value.Length < rule.CharCountMin))
            {
                return new string[] { "propertyMinLenghtValidatorError" };
            }

            if (_next != null)
                errors.AddRange(_next.Validate(rule, propertyValue));

            return errors.ToArray();
        }
    }
}
