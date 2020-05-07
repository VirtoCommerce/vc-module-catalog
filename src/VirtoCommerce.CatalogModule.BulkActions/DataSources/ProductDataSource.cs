using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.BulkActions.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.BulkActions.DataSources
{
    public class ProductDataSource : BaseDataSource
    {
        private readonly IListEntrySearchService _listEntrySearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDataSource"/> class.
        /// </summary>
        /// <param name="listEntrySearchService">
        /// The list entry search service.
        /// </param>
        /// <param name="dataQuery">
        /// The data query.
        /// </param>
        public ProductDataSource(IListEntrySearchService listEntrySearchService, DataQuery dataQuery)
            : base(listEntrySearchService, dataQuery)
        {
            _listEntrySearchService = listEntrySearchService;
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

            var categoryIds = entries.Where(entry => entry.Type.EqualsInvariant(CategoryListEntry.TypeName))
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

            var products = entries.Where(entry => entry.Type.EqualsInvariant(ProductListEntry.TypeName)).Skip(skip)
                .Take(take).ToArray();
            result.AddRange(products);

            return result;
        }

        protected override async Task<int> GetEntitiesCountAsync(ListEntryBase[] entries)
        {
            var inCategoriesCount = 0;
            var categoryIds = entries.Where(entry => entry.Type.EqualsInvariant(ProductListEntry.TypeName))
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
            var productCount = entries.Count(entry => entry.Type.EqualsInvariant(ProductListEntry.TypeName));

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

            var result = await _listEntrySearchService.SearchAsync(searchCriteria);
            return result;
        }
    }
}
