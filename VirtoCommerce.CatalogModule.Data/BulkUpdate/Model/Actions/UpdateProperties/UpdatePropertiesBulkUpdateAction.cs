using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

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

            var productIds = listEntries.Cast<ListEntryProduct>().Select(x => x.Id).ToArray();
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

        protected virtual bool SetCustomProperty(CatalogProduct product, Web.Model.Property propertyToSet, object valueToSet)
        {
            var result = false;

            var productPropertyValue = product.PropertyValues?.FirstOrDefault(x => x.Id.EqualsInvariant(propertyToSet.Id));

            if (productPropertyValue != null)
            {
                productPropertyValue.Value = valueToSet;
                result = true;
            }
            else
            {
                var property = product.Properties.FirstOrDefault(x => x.Id.EqualsInvariant(propertyToSet.Id));

                if (property != null)
                {
                    // Need to add product.PropertyValue here for that property
                    result = true;
                }
            }

            return result;
        }

        protected virtual bool SetOwnProperty(CatalogProduct product, Web.Model.Property propertyToSet, object valueToSet)
        {
            var result = false;
            // Need to find product property by name and assign the value to it

            return result;
        }
    }
}
