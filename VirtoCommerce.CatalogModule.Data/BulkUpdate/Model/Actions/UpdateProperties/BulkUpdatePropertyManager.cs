using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using web = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class BulkUpdatePropertyManager : IBulkUpdatePropertyManager
    {
        private readonly IPagedDataSourceFactory _dataSourceFactory;
        private readonly IItemService _itemService;

        private readonly Dictionary<string, MethodInfo> _productProperties = new Dictionary<string, MethodInfo>();


        public BulkUpdatePropertyManager(IPagedDataSourceFactory dataSourceFactory, IItemService itemService)
        {
            _dataSourceFactory = dataSourceFactory;
            _itemService = itemService;
        }

        public virtual Property[] GetProperties(UpdatePropertiesActionContext context)
        {
            // TechDebt: Should get all product inherited properties faster, by getting all properties for category line entries (including outline) + all inherited product line entry properties
            var dataSource = _dataSourceFactory.Create(context);
            var result = new List<Property>();
            var propertyIds = new HashSet<string>();

            result.AddRange(GetStandardProperties());

            while (dataSource.Fetch())
            {
                var productIds = dataSource.Items.Select(x => x.Id).ToArray();
                var products = _itemService.GetByIds(productIds, ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties);
                // Using only product inherited properties from categories, own product props (only from PropertyValues) are not set via bulk update 
                var newProperties = products
                    .SelectMany(x => x.Properties.Where(y => y.IsInherited))
                    .Distinct(AnonymousComparer.Create<Property, string>(x => x.Id))
                    .Where(x => !propertyIds.Contains(x.Id))
                    .ToArray();

                propertyIds.AddRange(newProperties.Select(x => x.Id));
                result.AddRange(newProperties);
            }

            return result.ToArray();
        }

        public virtual UpdatePropertiesResult UpdateProperties(CatalogProduct[] products, web.Property[] propertiesToSet)
        {
            var result = new UpdatePropertiesResult() { Succeeded = true };
            var hasChanges = false;

            if (!products.IsNullOrEmpty())
            {
                hasChanges = ChangesProductPropertyValues(propertiesToSet, products, result);
            }

            if (hasChanges)
            {
                _itemService.Update(products);
            }

            return result;
        }

        protected virtual IEnumerable<Property> GetStandardProperties()
        {
            yield return new Property() { Name = nameof(CatalogProduct.Name), Type = PropertyType.Product, ValueType = PropertyValueType.LongText, Required = true };
            yield return new Property() { Name = nameof(CatalogProduct.StartDate), Type = PropertyType.Product, ValueType = PropertyValueType.DateTime, Required = true, };
            yield return new Property() { Name = nameof(CatalogProduct.EndDate), Type = PropertyType.Product, ValueType = PropertyValueType.DateTime, };
            yield return new Property() { Name = nameof(CatalogProduct.Priority), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, Required = true, };
            yield return new Property() { Name = nameof(CatalogProduct.EnableReview), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.IsActive), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.IsBuyable), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.TrackInventory), Type = PropertyType.Product, ValueType = PropertyValueType.Boolean, };
            yield return new Property() { Name = nameof(CatalogProduct.MinQuantity), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, };
            yield return new Property() { Name = nameof(CatalogProduct.MaxQuantity), Type = PropertyType.Product, ValueType = PropertyValueType.Integer, };
            yield return new Property() { Name = nameof(CatalogProduct.Vendor), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true };
            yield return new Property() { Name = nameof(CatalogProduct.WeightUnit), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true };
            yield return new Property() { Name = nameof(CatalogProduct.Weight), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.MeasureUnit), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true };
            yield return new Property() { Name = nameof(CatalogProduct.PackageType), Type = PropertyType.Product, ValueType = PropertyValueType.ShortText, Dictionary = true };
            yield return new Property() { Name = nameof(CatalogProduct.Height), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.Width), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.Length), Type = PropertyType.Product, ValueType = PropertyValueType.Number, };
            yield return new Property() { Name = nameof(CatalogProduct.TaxType), Type = PropertyType.Product, ValueType = PropertyValueType.Number, Dictionary = true };
        }

        protected virtual bool ChangesProductPropertyValues(web.Property[] propertiesToSet, CatalogProduct[] products, UpdatePropertiesResult updateResult)
        {
            var hasChanges = false;

            foreach (var product in products)
            {
                try
                {
                    foreach (var propertyToSet in propertiesToSet)
                    {
                        if (!string.IsNullOrEmpty(propertyToSet.Id))
                        {
                            hasChanges = SetCustomProperty(product, propertyToSet) || hasChanges;
                        }
                        else if (!string.IsNullOrEmpty(propertyToSet.Name))
                        {
                            hasChanges = SetOwnProperty(product, propertyToSet) || hasChanges;
                        }
                    }
                }
                catch (Exception e)
                {
                    updateResult.Succeeded = false;
                    updateResult.Errors.Add(e.Message);
                }
            }

            return hasChanges;
        }

        protected virtual bool SetCustomProperty(CatalogProduct product, web.Property propertyToSet)
        {
            bool result;

            if (propertyToSet.Multivalue)
            {
                var productPropertyValues = product.PropertyValues?.Where(x => x.Property != null && x.Property.Id.EqualsInvariant(propertyToSet.Id)).ToArray();

                if (!productPropertyValues.IsNullOrEmpty())
                {
#pragma warning disable S2259 // Null pointers should not be dereferenced
                    foreach (var productPropertyValue in productPropertyValues)
#pragma warning restore S2259 // Null pointers should not be dereferenced
                    {
                        product.PropertyValues?.Remove(productPropertyValue);
                    }
                }

                result = AddPropertyValues(product, propertyToSet);
            }
            else
            {
                var productPropertyValue = product.PropertyValues?.FirstOrDefault(x => x.Property != null && x.Property.Id.EqualsInvariant(propertyToSet.Id));

                if (productPropertyValue != null)
                {
                    var propertyValueToSet = propertyToSet.Values.FirstOrDefault();

                    productPropertyValue.Value = propertyValueToSet?.Value;

                    if (propertyToSet.Dictionary)
                    {
                        productPropertyValue.ValueId = propertyValueToSet?.ValueId;
                    }
                    result = true;
                }
                else
                {
                    result = AddPropertyValues(product, propertyToSet);
                }
            }
            return result;
        }

        private bool AddPropertyValues(CatalogProduct product, web.Property propertyToSet)
        {
            var property = product.Properties.FirstOrDefault(x => x.Id.EqualsInvariant(propertyToSet.Id));

            var result = false;
            if (property != null)
            {

                if (product.PropertyValues == null)
                {
                    product.PropertyValues = new List<PropertyValue>();
                }

                foreach (var propertyValue in propertyToSet.Values.Select(x => x.ToCoreModel()))
                {
                    propertyValue.Property = property;
                    propertyValue.PropertyId = property.Id;
                    propertyValue.PropertyName = property.Name;
                    product.PropertyValues.Add(propertyValue);
                }

                result = true;
            }

            return result;
        }

        protected virtual bool SetOwnProperty(CatalogProduct product, web.Property propertyToSet)
        {
            var result = false;
            var propertyValueToSet = propertyToSet.Values.FirstOrDefault();
            var valueToSet = propertyToSet.Dictionary ? propertyValueToSet?.ValueId : propertyValueToSet?.Value;
            var setter = GetProductPropertySetter(product, propertyToSet);

            if (setter != null)
            {
                if (valueToSet == null && propertyToSet.Required)
                {
                    throw new ArgumentException($"Property value is missing for required property \"{propertyToSet.Name}\".");
                }

                var convertedValue = valueToSet != null ? ConvertValue(propertyToSet.ValueType, valueToSet) : null;

                setter.Invoke(product, new object[] { convertedValue });
                result = true;
            }

            return result;
        }

        protected virtual MethodInfo GetProductPropertySetter(CatalogProduct product, web.Property propertyToSet)
        {
            MethodInfo result;
            var propertyName = propertyToSet.Name;

            if (!_productProperties.TryGetValue(propertyName, out result))
            {
                var productType = product.GetType();
                var productProperty = productType.GetProperty(propertyName);
                result = productProperty?.GetSetMethod();

                _productProperties.Add(propertyName, result);
            }
            return result;
        }

        protected virtual object ConvertValue(PropertyValueType valueType, object value)
        {
            object result;

            switch (valueType)
            {
                case PropertyValueType.LongText:
                    result = Convert.ToString(value);
                    break;
                case PropertyValueType.ShortText:
                    result = Convert.ToString(value);
                    break;
                case PropertyValueType.Number:
                    result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    break;
                case PropertyValueType.DateTime:
                    result = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                    break;
                case PropertyValueType.Boolean:
                    result = Convert.ToBoolean(value);
                    break;
                case PropertyValueType.Integer:
                    result = Convert.ToInt32(value);
                    break;
                case PropertyValueType.GeoPoint:
                    result = Convert.ToString(value);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
    }
}
