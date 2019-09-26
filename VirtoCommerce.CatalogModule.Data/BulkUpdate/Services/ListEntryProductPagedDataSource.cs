using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.BulkUpdate.Model;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.BulkUpdate.Services
{
    public class ListEntryProductPagedDataSource : ListEntryPagedDataSource
    {
        private readonly IListEntrySearchService _listEntrySearchService;

        public ListEntryProductPagedDataSource(IListEntrySearchService listEntrySearchService, ListEntryDataQuery dataQuery) : base(listEntrySearchService, dataQuery)
        {
            _listEntrySearchService = listEntrySearchService;
        }

        protected override IEnumerable<IEntity> GetEntities(IEnumerable<ListEntry> listEntries, int skip, int take)
        {
            var result = new List<IEntity>();
            var categoryProductsSkip = 0;
            var categoryProductsTake = 0;

            // Find all products inside category list entries
            var categoryIds = listEntries.Where(x => x.Type.EqualsInvariant(ListEntryCategory.TypeName)).Select(x => x.Id).ToArray();
            if (!categoryIds.IsNullOrEmpty())
            {
                var searchResult = SearchProductsInCategories(categoryIds, 0, 0);
                var productsInCategoriesTotalCount = searchResult.TotalCount;

                categoryProductsSkip = Math.Min(productsInCategoriesTotalCount, skip);
                categoryProductsTake = Math.Min(take, Math.Max(0, productsInCategoriesTotalCount - skip));

                if (productsInCategoriesTotalCount > 0 && categoryProductsTake > 0)
                {
                    searchResult = SearchProductsInCategories(categoryIds, categoryProductsSkip, categoryProductsTake);
                    result.AddRange(searchResult.ListEntries);
                }
            }

            skip -= categoryProductsSkip;
            take -= categoryProductsTake;

            var products = listEntries.Where(x => x.Type.EqualsInvariant(ListEntryProduct.TypeName))
                .Skip(skip)
                .Take(take)
                .ToArray();
            result.AddRange(products);

            return result;
        }

        protected override int GetEntitiesCount(IEnumerable<ListEntry> listEntries)
        {
            // Find all products inside category list entries
            var productsInCategoriesTotalCount = 0;
            var categoryIds = listEntries.Where(x => x.Type.EqualsInvariant(ListEntryCategory.TypeName)).Select(x => x.Id).ToArray();

            if (!categoryIds.IsNullOrEmpty())
            {
                var searchResult = SearchProductsInCategories(categoryIds, 0, 0);
                productsInCategoriesTotalCount = searchResult.TotalCount;
            }

            // Find product list entry count
            var productCount = listEntries.Count(x => x.Type.EqualsInvariant(ListEntryProduct.TypeName));

            return productsInCategoriesTotalCount + productCount;
        }

        protected override domain.SearchCriteria BuildSearchCriteria(ListEntryDataQuery dataQuery)
        {
            var result = base.BuildSearchCriteria(dataQuery);

            result.ResponseGroup = domain.SearchResponseGroup.WithProducts;
            result.SearchInChildren = true;

            return result;
        }

        protected virtual ListEntrySearchResult SearchProductsInCategories(string[] categoryIds, int skip, int take)
        {
            var searchCriteria = new domain.SearchCriteria()
            {
                CategoryIds = categoryIds,
                Skip = skip,
                Take = take,
                ResponseGroup = domain.SearchResponseGroup.WithProducts,
                SearchInChildren = true,
                SearchInVariations = true,
            };

            return _listEntrySearchService.Search(searchCriteria);
        }

    }
}
