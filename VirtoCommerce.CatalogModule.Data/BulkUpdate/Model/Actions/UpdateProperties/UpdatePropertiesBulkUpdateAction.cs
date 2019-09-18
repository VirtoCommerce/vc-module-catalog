using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using Property = VirtoCommerce.CatalogModule.Web.Model.Property;
using PropertyValue = VirtoCommerce.Domain.Catalog.Model.PropertyValue;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class UpdatePropertiesBulkUpdateAction : IBulkUpdateAction
    {
        private readonly UpdatePropertiesActionContext _context;
        private readonly IBulkUpdatePropertyManager _bulkUpdatePropertyManager;
        private readonly IItemService _itemService;

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

        protected virtual bool ChangesProductPropertyValues(Web.Model.Property[] propertiesToSet, CatalogProduct[] products, BulkUpdateActionResult result)
        {
            var hasChanges = false;

            foreach (var product in products)
            {
                try
                {
                    foreach (var propertyToSet in propertiesToSet)
                    {
                        var valueToSet = propertyToSet.Multivalue ? propertyToSet.Values : propertyToSet.Values.FirstOrDefault()?.Value;

                        if (!string.IsNullOrEmpty(propertyToSet.Id))
                        {
                            hasChanges = SetCustomProperty(product, propertyToSet, valueToSet) || hasChanges;
                        }
                        else if (!string.IsNullOrEmpty(propertyToSet.Name))
                        {
                            hasChanges = SetOwnProperty(product, propertyToSet, valueToSet) || hasChanges;
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

        protected virtual bool SetCustomProperty(CatalogProduct product, Property propertyToSet, object valueToSet)
        {
            bool result;

            if (propertyToSet.Multivalue)
            {
                var productPropertyValues = product.PropertyValues?.Where(x => x.Property != null && x.Property.Id.EqualsInvariant(propertyToSet.Id)).ToArray();

                if (!productPropertyValues.IsNullOrEmpty())
                {
                    foreach (var productPropertyValue in productPropertyValues)
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
                    productPropertyValue.Value = valueToSet;
                    result = true;
                }
                else
                {
                    result = AddPropertyValues(product, propertyToSet);
                }
            }
            return result;
        }

        private bool AddPropertyValues(CatalogProduct product, Property propertyToSet)
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

        protected virtual bool SetOwnProperty(CatalogProduct product, Web.Model.Property propertyToSet, object valueToSet)
        {
            var result = true;
            // Need to find product property by name and assign the value to it

            return result;
        }
    }
}
