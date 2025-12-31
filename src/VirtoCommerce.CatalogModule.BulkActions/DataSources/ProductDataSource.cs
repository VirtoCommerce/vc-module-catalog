using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.BulkActionsModule.Core.Services;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.DataSources
{
    public class PropertyDataSource : IDataSource
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly ICategoryService _categoryService;
        private readonly IPropertyService _propertyService;
        private readonly IItemService _itemService;
        private readonly DataQuery _dataQuery;

        private bool _fetchExecuted = false;

        private readonly IEqualityComparer<Property> _propertyComparer = AnonymousComparer.Create<Property, string>(p => p.Id);

        public PropertyDataSource(Func<ICatalogRepository> repositoryFactory,
            ICategoryService categoryService,
            IPropertyService propertyService,
            IItemService productService,
            DataQuery dataQuery)
        {
            _repositoryFactory = repositoryFactory;
            _categoryService = categoryService;
            _propertyService = propertyService;
            _itemService = productService;
            _dataQuery = dataQuery ?? throw new ArgumentNullException(nameof(dataQuery));
        }

        public IEnumerable<IEntity> Items { get; set; }

        public async Task<bool> FetchAsync()
        {
            if (_fetchExecuted)
            {
                return false;
            }

            if (_dataQuery.ListEntries.IsNullOrEmpty())
            {
                if (_dataQuery.SearchCriteria == null)
                {
                    Items = Array.Empty<IEntity>();
                }
                else
                {
                    var catalogIds = _dataQuery.SearchCriteria.CatalogIds?.ToList() ?? [];
                    var categoryIds = _dataQuery.SearchCriteria.CategoryIds?.ToList() ?? [];

                    var categories = await _categoryService.GetNoCloneAsync(categoryIds, CategoryResponseGroup.WithProperties.ToString());

                    var result = new List<Property>();

                    foreach (var catalogId in catalogIds)
                    {
                        var catalogCategories = categories
                            .Where(c => c.CatalogId == catalogId)
                            .ToArray();

                        var properties = await GetCategoriesPropertiesAsync(catalogId, catalogCategories);
                        result.AddRange(properties);
                    }

                    Items = result;
                }
            }
            else
            {
                Items = await GetProductPropertiesAsync(_dataQuery?.ListEntries);
            }

            _fetchExecuted = true;

            return Items.Any();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new InvalidOperationException("GetTotalCountAsync is not supported in PropertyDataSource. Obtains all Properties in one go.");
        }

        private async Task<List<Property>> GetProductPropertiesAsync(ListEntryBase[] listEntries)
        {
            var result = new List<Property>();

            var entries = listEntries?.ToList() ?? [];

            if (entries.Count == 0)
            {
                return result;
            }

            var categoryEntries = entries
                .Where(entry => entry.Type.EqualsIgnoreCase(CategoryListEntry.TypeName))
                .ToArray();

            if (!categoryEntries.IsNullOrEmpty())
            {
                var categories = await _categoryService.GetNoCloneAsync(categoryEntries.Select(c => c.Id).ToArray(), CategoryResponseGroup.WithProperties.ToString());

                var catalogIds = categoryEntries.Select(entry => entry.CatalogId).Distinct().ToList();
                foreach (var catalogId in catalogIds)
                {
                    var catalogCategories = categories
                        .Where(c => c.CatalogId == catalogId)
                        .ToArray();

                    var properties = await GetCategoriesPropertiesAsync(catalogId, catalogCategories);
                    result.AddRange(properties);
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
                var newProperties = products.SelectMany(PropertiesSelector(isInherited: true))
                    .Distinct(_propertyComparer)
                    .Where(property => !propertyIds.Contains(property.Id)).ToArray();

                result.AddRange(newProperties);
            }

            return result;
        }

        private async Task<List<Property>> GetCategoriesPropertiesAsync(string catalogId, IList<Category> catalogCategories)
        {
            var properties = catalogCategories
                 .SelectMany(PropertiesSelector())
                 .ToList();

            using var repository = _repositoryFactory();

            var catalogCategoryIds = catalogCategories.Select(c => c.Id).ToArray();
            var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(catalogCategoryIds.ToArray());

            if (!childrenCategoryIds.IsNullOrEmpty())
            {
                var catalogProperties = await _propertyService.GetAllCatalogPropertiesAsync(catalogId);
                var childrenProperties = catalogProperties
                 .Where(x => childrenCategoryIds.Contains(x.CategoryId) && x.Type != PropertyType.Category)
                 .ToArray();

                properties.AddRange(childrenProperties);

            }

            properties = properties.Distinct(_propertyComparer).ToList();

            return properties;
        }

        private static Func<IHasProperties, IEnumerable<Property>> PropertiesSelector(bool? isInherited = null)
        {
            if (isInherited == null)
            {
                return x => x.Properties.Where(property => property.Id != null && property.Type != PropertyType.Category);
            }

            return x => x.Properties.Where(property => property.Id != null && property.IsInherited == isInherited && property.Type != PropertyType.Category);
        }
    }

    public class ProductDataSource : BaseDataSource
    {
        private readonly IInternalListEntrySearchService _internalListEntrySearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDataSource"/> class.
        /// </summary>
        /// <param name="listEntrySearchService">
        /// The list entry search service.
        /// </param>
        /// <param name="dataQuery">
        /// The data query.
        /// </param>
        public ProductDataSource(IInternalListEntrySearchService internalListEntrySearchService, DataQuery dataQuery)
            : base(internalListEntrySearchService, dataQuery)
        {
            _internalListEntrySearchService = internalListEntrySearchService;
        }

        protected override CatalogListEntrySearchCriteria BuildSearchCriteria(DataQuery dataQuery)
        {
            var result = base.BuildSearchCriteria(dataQuery);
            result.ResponseGroup = ItemResponseGroup.Full.ToString();
            result.SearchInChildren = true;
            return result;
        }

        protected override async Task<IEnumerable<IEntity>> GetNextItemsAsync(ListEntryBase[] entries, int skip,
            int take)
        {
            var result = new List<IEntity>();
            var categoryProductsSkip = 0;
            var categoryProductsTake = 0;

            var categoryIds = entries.Where(entry => entry.Type.EqualsIgnoreCase(CategoryListEntry.TypeName))
                .Select(entry => entry.Id).ToArray();
            if (categoryIds.IsNullOrEmpty())
            {
                // idle
            }
            else
            {
                // find all products inside category entries
                var searchResult = await SearchProductsInCategoriesAsync(categoryIds, 0, 0);
                var inCategoriesCount = searchResult.TotalCount;

                categoryProductsSkip = Math.Min(inCategoriesCount, skip);
                categoryProductsTake = Math.Min(take, Math.Max(0, inCategoriesCount - skip));

                if (inCategoriesCount > 0 && categoryProductsTake > 0)
                {
                    searchResult = await SearchProductsInCategoriesAsync(categoryIds, categoryProductsSkip, categoryProductsTake);
                    result.AddRange(searchResult.ListEntries);
                }
            }

            skip -= categoryProductsSkip;
            take -= categoryProductsTake;

            var products = entries.Where(entry => entry.Type.EqualsIgnoreCase(ProductListEntry.TypeName)).Skip(skip)
                .Take(take).ToArray();
            result.AddRange(products);

            return result;
        }

        protected override async Task<int> GetEntitiesCountAsync(ListEntryBase[] entries)
        {
            var inCategoriesCount = 0;
            var categoryIds = entries.Where(entry => entry.Type.EqualsIgnoreCase(ProductListEntry.TypeName))
                .Select(entry => entry.Id).ToArray();

            if (categoryIds.IsNullOrEmpty())
            {
                // idle
            }
            else
            {
                // find all products inside category entries
                var searchResult = await SearchProductsInCategoriesAsync(categoryIds, 0, 0);
                inCategoriesCount = searchResult.TotalCount;
            }

            // find product list entry count
            var productCount = entries.Count(entry => entry.Type.EqualsIgnoreCase(ProductListEntry.TypeName));

            return inCategoriesCount + productCount;
        }

        private async Task<ListEntrySearchResult> SearchProductsInCategoriesAsync(string[] categoryIds, int skip, int take)
        {
            var searchCriteria = AbstractTypeFactory<CatalogListEntrySearchCriteria>.TryCreateInstance();
            searchCriteria.CategoryIds = categoryIds;
            searchCriteria.Skip = skip;
            searchCriteria.Take = take;
            searchCriteria.ResponseGroup = ItemResponseGroup.Full.ToString();
            searchCriteria.SearchInChildren = true;
            searchCriteria.SearchInVariations = true;

            var result = await _internalListEntrySearchService.InnerListEntrySearchAsync(searchCriteria);
            return result;
        }
    }
}
