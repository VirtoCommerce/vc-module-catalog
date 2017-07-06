using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public interface IValidationService
    {
        Dictionary<PropertyValue, string[]> ValidatePropertyValues(ICollection<PropertyValue> propertyValues);
    }

    public class ValidationService : IValidationService
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyValueValidator _propertyValidator;

        public ValidationService(IPropertyService propertyService, IPropertyValueValidator propertyValidator)
        {
            _propertyService = propertyService;
            _propertyValidator = propertyValidator;
        }

        public Dictionary<PropertyValue, string[]> ValidatePropertyValues(ICollection<PropertyValue> propertyValues)
        {
            var result = new Dictionary<PropertyValue, string[]>();

            var propertiesId = propertyValues.Select(x => x.PropertyId).Distinct().ToArray();
            var properties = _propertyService.GetByIds(propertiesId);
            foreach (var propertyValue in propertyValues)
            {
                var property = properties.FirstOrDefault(x => x.Id == propertyValue.PropertyId);
                var rule = property.ValidationRules.FirstOrDefault();
                var errors = _propertyValidator.Validate(rule, propertyValue);
                if (errors.Any())
                {
                    result.Add(propertyValue, errors);
                }
            }

            return result;
        }
    }
}
