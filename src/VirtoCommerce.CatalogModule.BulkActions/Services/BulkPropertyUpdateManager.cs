using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.Services
{
    public class BulkPropertyUpdateManager : IBulkPropertyUpdateManager
    {
        private readonly IPropertyUpdateManager _propertyUpdateManager;
        private readonly IPropertyService _propertyService;

        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;

        private readonly Dictionary<string, string> _namesById = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkPropertyUpdateManager"/> class.
        /// </summary>
        /// <param name="dataSourceFactory">
        /// The data source factory.
        /// </param>
        /// <param name="itemService">
        /// The item service.
        /// </param>
        /// <param name="categoryService"></param>
        /// <param name="catalogService"></param>
        /// <param name="propertyUpdateManager"></param>
        public BulkPropertyUpdateManager(IDataSourceFactory dataSourceFactory, IItemService itemService, ICategoryService categoryService, ICatalogService catalogService, IPropertyUpdateManager propertyUpdateManager, IPropertyService propertyService)
        {
            _itemService = itemService;
            _categoryService = categoryService;
            _catalogService = catalogService;
            _propertyUpdateManager = propertyUpdateManager;
            _propertyService = propertyService;
        }

        public async Task<Property[]> GetPropertiesAsync(BulkActionContext context)
        {
            var result = new List<Property>();

            result.AddRange(_propertyUpdateManager.GetStandardProperties());

            if (context is not BaseBulkActionContext baseContext)
            {
                return result.ToArray();
            }

            var entries = baseContext.DataQuery?.ListEntries.ToList() ?? [];

            if (entries.Count == 0)
            {
                return result.ToArray();
            }

            var categories = entries
                .Where(entry => entry.Type.EqualsIgnoreCase(CategoryListEntry.TypeName))
                .ToArray();

            if (!categories.IsNullOrEmpty())
            {
                var catalogIds = categories.Select(entry => entry.CatalogId).Distinct().ToList();

                foreach (var catalogId in catalogIds)
                {
                    var catalogCategories = categories.Where(c => c.CatalogId == catalogId).ToList();

                    var catalogProperties = await _propertyService.GetCategoriesPropertiesAsync(catalogId, catalogCategories.Select(x => x.Id).ToArray());
                    catalogProperties = catalogProperties.Where(x => x.Type == PropertyType.Product).ToList();
                    result.AddRange(catalogProperties);
                }
            }

            var productIds = entries
                .Where(entry => entry.Type.EqualsIgnoreCase(ProductListEntry.TypeName))
                .Select(entry => entry.Id)
                .ToArray();

            if (!productIds.IsNullOrEmpty())
            {
                var propertyIds = result.Select(p => p.Id).ToHashSet();

                var products = await _itemService.GetAsync(productIds, (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString());

                // using only product inherited properties from categories,
                // own product props (only from PropertyValues) are not set via bulk update action
                var newProperties = products.SelectMany(CollectionSelector())
                    .Distinct(AnonymousComparer.Create<Property, string>(property => property.Id))
                    .Where(property => !propertyIds.Contains(property.Id)).ToArray();

                result.AddRange(newProperties);
            }

            foreach (var property in result)
            {
                await FillOwnerName(property);
            }

            return result.ToArray();
        }

        public async Task<BulkActionResult> UpdatePropertiesAsync(CatalogProduct[] products, Property[] properties)
        {
            var result = new BulkActionResult { Succeeded = true };
            var hasChanges = false;

            if (products.IsNullOrEmpty())
            {
                // idle
            }
            else
            {
                hasChanges = TryChangeProductPropertyValues(properties, products, result);
            }

            if (hasChanges)
            {
                await _itemService.SaveChangesAsync(products);
            }

            return result;
        }

        private async Task FillOwnerName(Property property)
        {
            string ownerName;

            if (!string.IsNullOrEmpty(property.CategoryId))
            {
                if (!_namesById.TryGetValue(property.CategoryId, out ownerName))
                {
                    var category = await _categoryService.GetNoCloneAsync(property.CategoryId, CategoryResponseGroup.Info.ToString());
                    ownerName = $"{category?.Name} (Category)";
                    _namesById.Add(property.CategoryId, ownerName);
                }
            }
            else if (!string.IsNullOrEmpty(property.CatalogId))
            {
                if (!_namesById.TryGetValue(property.CatalogId, out ownerName))
                {
                    var catalog = await _catalogService.GetNoCloneAsync(property.CatalogId, CategoryResponseGroup.Info.ToString());
                    ownerName = $"{catalog?.Name} (Catalog)";
                    _namesById.Add(property.CatalogId, ownerName);
                }
            }
            else
            {
                ownerName = "Native properties";
            }

            property.OwnerName = ownerName;
        }

        private static Func<CatalogProduct, IEnumerable<Property>> CollectionSelector()
        {
            return product =>
                product.Properties.Where(property => property.Id != null && property.IsInherited && property.Type != PropertyType.Category);
        }

        private bool TryChangeProductPropertyValues(
            Property[] properties,
            IEnumerable<CatalogProduct> products,
            BulkActionResult result)
        {
            var hasChanges = false;

            foreach (var product in products)
            {
                try
                {
                    hasChanges |= _propertyUpdateManager.TryChangeProductPropertyValues(product, properties);
                }
                catch (Exception e)
                {
                    result.Succeeded = false;
                    result.Errors.Add(e.Message);
                }
            }

            return hasChanges;
        }

    }
}
