using System;
using System.Collections.Generic;
using System.Linq;

using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

using Flags = VirtoCommerce.Domain.Catalog.Model.SearchResponseGroup;
using SearchCriteria = VirtoCommerce.Domain.Catalog.Model.SearchCriteria;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ListEntrySearchService : IListEntrySearchService
    {
        private readonly IBlobUrlResolver _blobUrlResolver;

        private readonly ICatalogSearchService _searchService;

        public ListEntrySearchService(ICatalogSearchService searchService, IBlobUrlResolver blobUrlResolver)
        {
            _searchService = searchService;
            _blobUrlResolver = blobUrlResolver;
        }

        public ListEntrySearchResult Search(SearchCriteria criteria)
        {
            var result = new ListEntrySearchResult();
            var skip = 0;
            var take = 0;

            // because products and categories represent in search result as two separated collections for handle paging request 
            // we should join two resulting collection artificially

            // search categories
            var responseGroupCopy = criteria.ResponseGroup;
            if (criteria.ResponseGroup.HasFlag(Flags.WithCategories))
            {
                criteria.ResponseGroup &= ~Flags.WithProducts;

                var searchResult = _searchService.Search(criteria);
                var totalCount = searchResult.Categories.Count;
                skip = GetSkip(criteria, totalCount);
                take = GetTake(criteria, totalCount);
                var pagedCategories = searchResult.Categories.Skip(skip).Take(take);
                var categories = pagedCategories.Select(
                    category => new ListEntryCategory(category.ToWebModel(_blobUrlResolver)));
                result.TotalCount = totalCount;

                result.ListEntries.AddRange(categories);
            }

            criteria.ResponseGroup = responseGroupCopy;

            // search products
            if (criteria.ResponseGroup.HasFlag(Flags.WithProducts))
            {
                criteria.ResponseGroup &= ~Flags.WithCategories;

                criteria.Skip -= skip;
                criteria.Take -= take;
                var searchResult = _searchService.Search(criteria);
                var products = searchResult.Products.Select(
                    product => new ListEntryProduct(product.ToWebModel(_blobUrlResolver)));
                result.TotalCount += searchResult.ProductsTotalCount;

                result.ListEntries.AddRange(products);
            }

            return result;
        }

        private static int GetSkip(SearchCriteria criteria, int totalCount)
        {
            return Math.Min(totalCount, criteria.Skip);
        }

        private static int GetTake(SearchCriteria criteria, int totalCount)
        {
            return Math.Min(criteria.Take, Math.Max(0, totalCount - criteria.Skip));
        }
    }
}
