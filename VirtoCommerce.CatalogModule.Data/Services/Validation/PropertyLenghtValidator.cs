using System.Collections.Generic;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class PropertyLenghtValidator : IPropertyValueValidator
    {
        private readonly IPropertyValueValidator _next;

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
                return new[] { "propertyRangeLenghtValidatorError" };
            }

            if ((rule.CharCountMax.HasValue && !rule.CharCountMin.HasValue) &&
                (value.Length > rule.CharCountMax))
            {
                return new[] { "propertyMaxLenghtValidatorError" };
            }

            if ((!rule.CharCountMax.HasValue && rule.CharCountMin.HasValue) &&
                (value.Length < rule.CharCountMin))
            {
                return new[] { "propertyMinLenghtValidatorError" };
            }

            if (_next != null)
                errors.AddRange(_next.Validate(rule, propertyValue));

            return errors.ToArray();
        }
    }
}
