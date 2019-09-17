using System;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ListEntrySearchService : IListEntrySearchService
    {
        private readonly ICatalogSearchService _searchService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public ListEntrySearchService(ICatalogSearchService searchService, IBlobUrlResolver blobUrlResolver)
        {
            _searchService = searchService;
            _blobUrlResolver = blobUrlResolver;
        }

        public ListEntrySearchResult Search(VirtoCommerce.Domain.Catalog.Model.SearchCriteria criteria)
        {
            var result = new ListEntrySearchResult();
            var categorySkip = 0;
            var categoryTake = 0;

            //Because products and categories represent in search result as two separated collections for handle paging request 
            //we should join two resulting collection artificially
            //search categories
            var copyRespGroup = criteria.ResponseGroup;
            if ((criteria.ResponseGroup & SearchResponseGroup.WithCategories) == SearchResponseGroup.WithCategories)
            {
                criteria.ResponseGroup = criteria.ResponseGroup & ~SearchResponseGroup.WithProducts;
                var categoriesSearchResult = _searchService.Search(criteria);
                var categoriesTotalCount = categoriesSearchResult.Categories.Count;

                categorySkip = Math.Min(categoriesTotalCount, criteria.Skip);
                categoryTake = Math.Min(criteria.Take, Math.Max(0, categoriesTotalCount - criteria.Skip));
                var categories = categoriesSearchResult.Categories.Skip(categorySkip).Take(categoryTake).Select(x => new ListEntryCategory(x.ToWebModel(_blobUrlResolver))).ToList();

                result.TotalCount = categoriesTotalCount;
                result.ListEntries.AddRange(categories);
            }
            criteria.ResponseGroup = copyRespGroup;
            //search products
            if ((criteria.ResponseGroup & SearchResponseGroup.WithProducts) == SearchResponseGroup.WithProducts)
            {
                criteria.ResponseGroup = criteria.ResponseGroup & ~SearchResponseGroup.WithCategories;
                criteria.Skip = criteria.Skip - categorySkip;
                criteria.Take = criteria.Take - categoryTake;
                var productsSearchResult = _searchService.Search(criteria);

                var products = productsSearchResult.Products.Select(x => new ListEntryProduct(x.ToWebModel(_blobUrlResolver)));

                result.TotalCount += productsSearchResult.ProductsTotalCount;
                result.ListEntries.AddRange(products);
            }

            return result;
        }
    }
}
