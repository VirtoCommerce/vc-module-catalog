using System;
using System.Linq;
using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class PropertyUniquenessValidator : IPropertyValueValidator
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly IPropertyValueValidator _next;

        public PropertyUniquenessValidator(Func<ICatalogRepository> repositoryFactory, IPropertyValueValidator validator)
        {
            _repositoryFactory = repositoryFactory;
            _next = validator;
        }

        public string[] Validate(PropertyValidationRule rule, PropertyValue propertyValue)
        {
            var errors = new List<string>();

            if (rule.IsUnique)
            {
                //find same property value for property
                using (var repository = _repositoryFactory())
                {
                    var propertyName = rule.Property.Name;
                    var query = repository.PropertyValues.Where(x => x.Name == propertyName);

                    bool any = false;
                    if (propertyValue.ValueType == PropertyValueType.LongText)
                    {
                        var textValue = propertyValue.Value.ToString();
                        any  = query.Any(x => x.LongTextValue == textValue);
                    }
                    if (propertyValue.ValueType == PropertyValueType.ShortText)
                    {
                        var textValue = propertyValue.Value.ToString();
                        any = query.Any(x => x.ShortTextValue == textValue);
                    }
                    if (propertyValue.ValueType == PropertyValueType.Number)
                    {
                        var decimalValue = Convert.ToDecimal(propertyValue.Value);
                        any = query.Any(x => x.DecimalValue == decimalValue);
                    }
                    if (propertyValue.ValueType == PropertyValueType.Integer)
                    {
                        var intValue = Convert.ToInt32(propertyValue.Value);
                        any = query.Any(x => x.IntegerValue == intValue);
                    }
                    if (propertyValue.ValueType == PropertyValueType.Boolean)
                    {
                        var boolValue = Convert.ToBoolean(propertyValue.Value);
                        any = query.Any(x => x.BooleanValue == boolValue);
                    }

                    if (any)
                    {
                        errors.Add("propertyUniquenessValidatorError");
                    }
                }
            }

            if (_next != null)
                errors.AddRange(_next.Validate(rule, propertyValue));

            return errors.ToArray();
        }
    }

}
