using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyUpdateManager : IPropertyUpdateManager
    {
        private readonly IPropertyDictionaryItemSearchService _propertyDictionarySearchService;

        private readonly Dictionary<string, MethodInfo> _productProperties = new();

        public PropertyUpdateManager(IPropertyDictionaryItemSearchService propertyDictionarySearchService)
        {
            _propertyDictionarySearchService = propertyDictionarySearchService;
        }

        public IList<Property> GetStandardProperties()
        {
            // TechDebt: Should get all product inherited properties faster,
            // by getting all properties for category line entries (including outline) + all inherited product line entry properties
            var result = new List<Property>
                         {
                             new Property { Name = nameof(CatalogProduct.Name), Type = PropertyType.Product, ValueType = PropertyValueType.LongText, Required = true },
                             new Property { Name = nameof(CatalogProduct.StartDate), Type = PropertyType.Product, ValueType = PropertyValueType.DateTime, Required = true },
                             new Property { Name = nameof(CatalogProduct.EndDate), Type = PropertyType.Product, ValueType = PropertyValueType.DateTime, },
                             new Property { Name = nameof(CatalogProduct.Priority), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, Required = true },
                             new Property { Name = nameof(CatalogProduct.EnableReview), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean },
                             new Property { Name = nameof(CatalogProduct.IsActive), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean },
                             new Property { Name = nameof(CatalogProduct.IsBuyable), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean },
                             new Property { Name = nameof(CatalogProduct.TrackInventory), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean },
                             new Property { Name = nameof(CatalogProduct.MinQuantity), Type = PropertyType.Product, ValueType = PropertyValueType.Integer },
                             new Property { Name = nameof(CatalogProduct.MaxQuantity), Type = PropertyType.Product, ValueType = PropertyValueType.Integer },
                             new Property { Name = nameof(CatalogProduct.Vendor), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true },
                             new Property { Name = nameof(CatalogProduct.WeightUnit), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true },
                             new Property { Name = nameof(CatalogProduct.Weight), Type = PropertyType.Product, ValueType = PropertyValueType.Number },
                             new Property { Name = nameof(CatalogProduct.MeasureUnit), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true },
                             new Property { Name = nameof(CatalogProduct.PackageType), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true },
                             new Property { Name = nameof(CatalogProduct.Height), Type = PropertyType.Product, ValueType = PropertyValueType.Number },
                             new Property { Name = nameof(CatalogProduct.Width), Type = PropertyType.Product, ValueType = PropertyValueType.Number },
                             new Property { Name = nameof(CatalogProduct.Length), Type = PropertyType.Product, ValueType = PropertyValueType.Number },
                             new Property { Name = nameof(CatalogProduct.TaxType), Type = PropertyType.Product, ValueType = PropertyValueType.Number, Dictionary = true }
                         };

            return result;
        }

        public bool TryChangeProductPropertyValues(CatalogProduct product, Property[] properties)
        {
            var hasChanges = false;
            foreach (var property in properties)
            {
                if (string.IsNullOrEmpty(property.Id))
                {
                    if (string.IsNullOrEmpty(property.Name))
                    {
                        continue;
                    }

                    hasChanges = TrySetOwnProperty(product, property) || hasChanges;
                }
                else
                {
                    hasChanges = TrySetCustomProperty(product, property) || hasChanges;
                }
            }

            return hasChanges;
        }

        public Task<ChangeProductPropertiesResult> TryChangeProductPropertyValues(CatalogProduct product, JObject source, string language)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            return TryChangeProductPropertyValuesInternal(product, source, language);
        }

        private async Task<ChangeProductPropertiesResult> TryChangeProductPropertyValuesInternal(CatalogProduct product, JObject source, string language)
        {
            var allProperties = product.Properties.Union(product.Category.Properties.Where(x => x.Type == PropertyType.Product)).Union(GetStandardProperties());
            var mapper = new PropertyValuesMapper(_propertyDictionarySearchService);

            var result = new ChangeProductPropertiesResult();

            foreach (var propertyName in source.Properties().Select(x => x.Name))
            {
                var property = allProperties.FirstOrDefault(x => x.Name.EqualsInvariant(propertyName));
                if (property == null)
                {
                    result.Errors.Add($"Property '{propertyName}' is not allowed.");
                }
                else
                {
                    mapper.AddProperty(property);
                }
            }

            if (!result.Succeeded)
            {
                return result;
            }

            var propertiesToSet = await mapper.GetResultProperties(product, source, language);

            foreach (var property in propertiesToSet)
            {
                if (!TrySetOwnProperty(product, property))
                {
                    TrySetCustomProperty(product, property);
                }
            }

            return result;
        }

        private static bool TrySetCustomProperty(IHasProperties product, Property property)
        {
            bool result;

            if (property.Multivalue || property.Dictionary)
            {
                var properties = product.Properties?.Where(prop => prop.Id.EqualsInvariant(property.Id)).ToArray();

                if (properties.IsNullOrEmpty())
                {
                    // Because no properties with same Id found
                    return AddPropertyValues(product, property);
                }

                Debug.Assert(properties != null);
                foreach (var productProperty in properties)
                {
                    productProperty.Values = new List<PropertyValue>();
                }

                result = AddPropertyValues(product, property);
            }
            else
            {
                var productProperty = product.Properties?.FirstOrDefault(prop => prop.Id.EqualsInvariant(property.Id));

                if (productProperty != null)
                {
                    productProperty.Values = property.Values;

                    result = true;
                }
                else
                {
                    result = AddPropertyValues(product, property);
                }
            }

            return result;
        }

        private bool TrySetOwnProperty(CatalogProduct product, Property property)
        {
            bool result;
            var propertyValue = property.Values.FirstOrDefault();
            var value = (property.Dictionary && !string.IsNullOrEmpty(propertyValue?.ValueId))
                ? propertyValue.ValueId
                : propertyValue?.Value;

            var setter = GetProductPropertySetter(product, property);

            if (setter == null)
            {
                result = false;
            }
            else
            {
                if (value == null && property.Required)
                {
                    var message = $"Property value is missing for required property \"{property.Name}\".";
                    throw new ArgumentException(message);
                }

                var convertedValue = value != null ? property.ValueType.ConvertValue(value) : null;

                setter.Invoke(product, new[] { convertedValue });
                result = true;
            }

            return result;
        }

        private MethodInfo GetProductPropertySetter(CatalogProduct product, Property property)
        {
            var name = property.Name;

            if (_productProperties.TryGetValue(name, out var result))
            {
                return result;
            }

            var productType = product.GetType();
            var productProperty = productType.GetProperty(name);
            result = productProperty?.GetSetMethod();

            _productProperties.Add(name, result);

            return result;
        }

        private static bool AddPropertyValues(IHasProperties product, Property property)
        {
            bool result;
            var foundProperty = product.Properties.FirstOrDefault(p => p.Id.EqualsInvariant(property.Id));
            if (foundProperty == null)
            {
                result = false;
            }
            else
            {
                foreach (var propertyValue in property.Values)
                {
                    propertyValue.Property = foundProperty;
                    propertyValue.PropertyId = foundProperty.Id;
                    propertyValue.PropertyName = foundProperty.Name;
                    foundProperty.Values.Add(propertyValue);
                }

                result = true;
            }

            return result;
        }
    }
}
