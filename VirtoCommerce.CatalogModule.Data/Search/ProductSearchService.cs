using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Aggregation = VirtoCommerce.CatalogModule.Web.Model.Aggregation;
using SearchCriteria = VirtoCommerce.Domain.Search.SearchCriteria;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchService : BaseSearchService<ProductSearch, Product, ProductSearchResult>, IProductSearchService
    {
        private readonly IItemService _itemService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IBrowseFilterService _browseFilterService;

        public ProductSearchService(ISearchProvider searchProvider, ISearchRequestBuilder[] searchRequestBuilders, IStoreService storeService, ISettingsManager settingsManager, IItemService itemService, IBlobUrlResolver blobUrlResolver, IBrowseFilterService browseFilterService)
            : base(searchProvider, searchRequestBuilders, storeService, settingsManager)
        {
            _browseFilterService = browseFilterService;
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
        }

        protected override SearchCriteria GetSearchCriteria(ProductSearch search, Store store)
        {
            var filters = GetPredefinedFilters(store);
            var result = search.AsCriteria<ProductSearchCriteria>(store.Catalog, filters);
            return result;
        }

        protected override IList<Product> LoadMissingItems(string[] missingItemIds, SearchCriteria searchCriteria, ProductSearch search)
        {
            var catalog = (searchCriteria as ProductSearchCriteria)?.Catalog;
            var products = _itemService.GetByIds(missingItemIds, GetResponseGroup(search), catalog);
            var result = products.Select(p => p.ToWebModel(_blobUrlResolver)).ToArray();
            return result;
        }

        protected virtual ItemResponseGroup GetResponseGroup(ProductSearch search)
        {
            var result = EnumUtility.SafeParse(search.ResponseGroup, ItemResponseGroup.ItemLarge & ~ItemResponseGroup.ItemProperties);
            return result;
        }

        protected override void ReduceSearchResults(IEnumerable<Product> items, ProductSearch search)
        {
            var responseGroup = GetResponseGroup(search);

            foreach (var obj in items)
            {
                ReduceSearchResult(obj, responseGroup);
            }
        }

        protected virtual void ReduceSearchResult(Product product, ItemResponseGroup responseGroup)
        {
            if (!responseGroup.HasFlag(ItemResponseGroup.ItemAssets))
            {
                product.Assets = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.ItemAssociations))
            {
                product.Associations = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
            {
                product.Reviews = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.ItemInfo))
            {
                product.Properties = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Links))
            {
                product.Links = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Outlines))
            {
                product.Outlines = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Seo))
            {
                product.SeoInfos = null;
            }

            if (!responseGroup.HasFlag(ItemResponseGroup.Variations))
            {
                product.Variations = null;
            }
        }

        protected virtual IList<ISearchFilter> GetPredefinedFilters(Store store)
        {
            var context = new Dictionary<string, object>
            {
                { "Store", store },
            };

            return _browseFilterService.GetFilters(context);
        }

        protected override Aggregation[] ConvertAggregations(IList<AggregationResponse> aggregations, SearchCriteria searchCriteria)
        {
            Aggregation[] result = null;

            if (aggregations?.Any() == true)
            {
            }

            return result;
        }
    }
}
