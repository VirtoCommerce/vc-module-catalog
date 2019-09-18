using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Services;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Model.Actions.UpdateProperties
{
    public class UpdatePropertiesBulkUpdateAction : IBulkUpdateAction
    {
        private readonly UpdatePropertiesActionContext _context;
        private readonly IBulkUpdatePropertyManager _bulkUpdatePropertyManager;

        public UpdatePropertiesBulkUpdateAction(IBulkUpdatePropertyManager bulkUpdatePropertyManager, UpdatePropertiesActionContext context)
        {
            _bulkUpdatePropertyManager = bulkUpdatePropertyManager;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BulkUpdateActionContext Context => _context;

        public IBulkUpdateActionData GetActionData()
        {
            var properties = _bulkUpdatePropertyManager.GetProperties(_context);

            return new UpdatePropertiesActionData()
            {
                Properties = properties.Select(x => x.ToWebModel()).ToArray(),
            };
        }

        public BulkUpdateActionResult Validate()
        {
            var result = BulkUpdateActionResult.Success;

            return result;
        }

        public BulkUpdateActionResult Execute(IEnumerable<IEntity> entities)
        {
            var result = BulkUpdateActionResult.Success;
            var propertiesToSet = _context.Properties;
            var products = entities.Cast<CatalogProduct>().ToArray();

            foreach (var product in products)
            {
                foreach (var propertyToSet in propertiesToSet)
                {
                    var valueToSet = propertyToSet.Multivalue ? propertyToSet.Values : propertyToSet.Values.FirstOrDefault()?.Value;

                    if (!string.IsNullOrEmpty(propertyToSet.Id))
                    {
                        SetCustomProperty(product, propertyToSet, valueToSet);
                    }
                    else if (!string.IsNullOrEmpty(propertyToSet.Name))
                    {
                        SetOwnProperty(product, propertyToSet, valueToSet);
                    }
                }
            }

            return result;

        }

        protected virtual void SetCustomProperty(CatalogProduct product, Web.Model.Property propertyToSet, object valueToSet)
        {
            var productPropertyValue = product.PropertyValues?.FirstOrDefault(x => x.Id.EqualsInvariant(propertyToSet.Id));

            if (productPropertyValue != null)
            {
                productPropertyValue.Value = valueToSet;
            }
            else
            {
                var property = product.Properties.FirstOrDefault(x => x.Id.EqualsInvariant(propertyToSet.Id));

                if (property != null)
                {
                    // Need to add product.PropertyValue here for that property
                }
            }
        }

        protected virtual void SetOwnProperty(CatalogProduct product, Web.Model.Property propertyToSet, object valueToSet)
        {
            // Need to find product property by name and assign the value to it
        }
    }
}
