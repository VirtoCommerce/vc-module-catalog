using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ICatalogService _catalogService;
        private readonly ICategoryService _categoryService;

        private readonly Dictionary<string, string> _namesById = new Dictionary<string, string>();

        public UpdatePropertiesBulkUpdateAction(IBulkUpdatePropertyManager bulkUpdatePropertyManager,
            IItemService itemService,
            ICatalogService catalogService,
            ICategoryService categoryService,
            UpdatePropertiesActionContext context)
        {
            _bulkUpdatePropertyManager = bulkUpdatePropertyManager;
            _itemService = itemService;
            _catalogService = catalogService;
            _categoryService = categoryService;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BulkUpdateActionContext Context => _context;

        public virtual IBulkUpdateActionData GetActionData()
        {
            var properties = _bulkUpdatePropertyManager.GetProperties(_context);

            return new UpdatePropertiesActionData()
            {
                Properties = properties.Select(x => CreateWebModel(x)).ToArray(),
            };
        }

        public virtual BulkUpdateActionResult Validate()
        {
            var result = BulkUpdateActionResult.Success;

            return result;
        }

        public virtual BulkUpdateActionResult Execute(IEnumerable<IEntity> entities)
        {
            var listEntries = entities.Cast<ListEntry>().ToArray();

            if (listEntries.Any(x => !x.Type.EqualsInvariant(ListEntryProduct.TypeName)))
            {
                throw new ArgumentException($"{GetType().Name} could be applied to product entities only.");
            }

            var productIds = listEntries.Where(x => x.Type.EqualsInvariant(ListEntryProduct.TypeName)).Select(x => x.Id).ToArray();
            var products = _itemService.GetByIds(productIds, ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties);

            return _bulkUpdatePropertyManager.UpdateProperties(products, _context.Properties);
        }

        protected virtual Web.Model.Property CreateWebModel(Domain.Catalog.Model.Property property)
        {
            var result = property.ToWebModel();
            string ownerName = null;

            if (!string.IsNullOrEmpty(property.CategoryId))
            {
                if (!_namesById.TryGetValue(property.CategoryId, out ownerName))
                {
                    ownerName = $"{_categoryService.GetById(property.CategoryId, CategoryResponseGroup.Info)?.Name} (Category)";
                    _namesById.Add(property.CategoryId, ownerName);
                }
            }
            else if (!string.IsNullOrEmpty(property.CatalogId))
            {
#pragma warning disable S1066 // Collapsible "if" statements should be merged
                if (!_namesById.TryGetValue(property.CatalogId, out ownerName))
#pragma warning restore S1066 // Collapsible "if" statements should be merged
                {
                    ownerName = $"{_catalogService.GetById(property.CatalogId)?.Name} (Catalog)";
                    _namesById.Add(property.CatalogId, ownerName);
                }
            }
            else
            {
                ownerName = "Native properties";
            }

            result.Path = ownerName;

            return result;
        }
    }
}
