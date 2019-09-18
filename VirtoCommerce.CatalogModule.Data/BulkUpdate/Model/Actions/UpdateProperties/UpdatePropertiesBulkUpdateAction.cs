using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;
using web = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class UpdatePropertiesBulkUpdateAction : IBulkUpdateAction
    {
        private readonly UpdatePropertiesActionContext _context;
        private readonly IBulkUpdatePropertyManager _bulkUpdatePropertyManager;
        private readonly IItemService _itemService;

        private readonly Dictionary<string, MethodInfo> _productProperties = new Dictionary<string, MethodInfo>();

        public UpdatePropertiesBulkUpdateAction(IBulkUpdatePropertyManager bulkUpdatePropertyManager,
            IItemService itemService,
            UpdatePropertiesActionContext context)
        {
            _bulkUpdatePropertyManager = bulkUpdatePropertyManager;
            _itemService = itemService;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BulkUpdateActionContext Context => _context;

        public virtual IBulkUpdateActionData GetActionData()
        {
            var properties = _bulkUpdatePropertyManager.GetProperties(_context);

            return new UpdatePropertiesActionData()
            {
                Properties = properties.Select(x => x.ToWebModel()).ToArray(),
            };
        }

        public virtual BulkUpdateActionResult Validate()
        {
            var result = BulkUpdateActionResult.Success;

            return result;
        }

        public virtual BulkUpdateActionResult Execute(IEnumerable<IEntity> entities)
        {
            var result = BulkUpdateActionResult.Success;
            var propertiesToSet = _context.Properties;
            var listEntries = entities.Cast<ListEntry>().ToArray();

            if (listEntries.Any(x => !x.Type.EqualsInvariant(ListEntryProduct.TypeName)))
            {
                throw new ArgumentException($"{GetType().Name} could be applied to product entities only.");
            }

            var productIds = listEntries.Where(x => x.Type.EqualsInvariant(ListEntryProduct.TypeName)).Select(x => x.Id).ToArray();
            var products = _itemService.GetByIds(productIds, ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties);
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

        protected virtual bool ChangesProductPropertyValues(web.Property[] propertiesToSet, CatalogProduct[] products, BulkUpdateActionResult result)
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
                    result.Succeeded = false;
                    result.Errors.Add(e.Message);
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

                    productPropertyValue.Value = propertyValueToSet.Value;

                    if (propertyToSet.Dictionary)
                    {
                        productPropertyValue.ValueId = propertyValueToSet.ValueId;
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
                    product.PropertyValues = new List<domain.PropertyValue>();
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
                var convertedValue = ConvertValue(propertyToSet.ValueType, valueToSet);

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
