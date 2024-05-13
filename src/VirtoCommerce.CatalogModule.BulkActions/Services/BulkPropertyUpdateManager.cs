using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Models.BulkActions;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.Services
{
    public class BulkPropertyUpdateManager : IBulkPropertyUpdateManager
    {
        private readonly IDataSourceFactory _dataSourceFactory;
        private readonly IPropertyUpdateManager _propertyUpdateManager;

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;

        private readonly Dictionary<string, string> _namesById = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkPropertyUpdateManager"/> class.
        /// </summary>
        /// <param name="dataSourceFactory">
        /// The data source factory.
        /// </param>
        /// <param name="productService">
        /// The item service.
        /// </param>
        /// <param name="categoryService"></param>
        /// <param name="catalogService"></param>
        /// <param name="propertyUpdateManager"></param>
        public BulkPropertyUpdateManager(IDataSourceFactory dataSourceFactory, IProductService productService, ICategoryService categoryService, ICatalogService catalogService, IPropertyUpdateManager propertyUpdateManager)
        {
            _dataSourceFactory = dataSourceFactory;
            _productService = productService;
            _categoryService = categoryService;
            _catalogService = catalogService;
            _propertyUpdateManager = propertyUpdateManager;
        }

        [Obsolete($"Use the overload that accepts {nameof(IProductService)}")]
        public BulkPropertyUpdateManager(IDataSourceFactory dataSourceFactory, IItemService itemService, ICategoryService categoryService, ICatalogService catalogService, IPropertyUpdateManager propertyUpdateManager)
            : this(dataSourceFactory, (IProductService)itemService, categoryService, catalogService, propertyUpdateManager)
        {
        }

        public async Task<Property[]> GetPropertiesAsync(BulkActionContext context)
        {
            var result = new List<Property>();
            var propertyIds = new HashSet<string>();
            var dataSource = _dataSourceFactory.Create(context);
            result.AddRange(_propertyUpdateManager.GetStandardProperties());

            while (await dataSource.FetchAsync())
            {
                var productIds = dataSource.Items.Select(item => item.Id).ToList();
                var products = await _productService.GetAsync(productIds, (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString());

                // using only product inherited properties from categories,
                // own product props (only from PropertyValues) are not set via bulk update action
                var newProperties = products.SelectMany(CollectionSelector())
                    .Distinct(AnonymousComparer.Create<Property, string>(property => property.Id))
                    .Where(property => !propertyIds.Contains(property.Id)).ToArray();

                propertyIds.AddRange(newProperties.Select(property => property.Id));
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
                await _productService.SaveChangesAsync(products);
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
