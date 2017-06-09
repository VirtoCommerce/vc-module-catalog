using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Aggregation = VirtoCommerce.Domain.Catalog.Model.Aggregation;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchService : IProductSearchService
    {
        private readonly IStoreService _storeService;
        private readonly IBrowseFilterService _browseFilterService;
        private readonly ISearchProvider _searchProvider;
        private readonly ISearchRequestBuilder[] _searchRequestBuilders;
        private readonly ISettingsManager _settingsManager;

        public ProductSearchService(IStoreService storeService, IBrowseFilterService browseFilterService, ISearchProvider searchProvider, ISearchRequestBuilder[] searchRequestBuilders, ISettingsManager settingsManager)
        {
            _storeService = storeService;
            _browseFilterService = browseFilterService;
            _searchProvider = searchProvider;
            _searchRequestBuilders = searchRequestBuilders;
            _settingsManager = settingsManager;
        }

        public async Task<ProductSearchResult> SearchProductsAsync(string storeId, ProductSearch productSearch)
        {
            var result = new ProductSearchResult();

            var store = _storeService.GetById(storeId);
            if (store != null)
            {
                var filters = GetPredefinedFilters(store);
                var responseGroup = EnumUtility.SafeParse(productSearch.ResponseGroup, ItemResponseGroup.ItemLarge & ~ItemResponseGroup.ItemProperties);
                var searchCriteria = productSearch.AsCriteria<ProductSearchCriteria>(storeId, store.Catalog, filters);

                var searchRequest = ConvertSearchCriteriaToSearchRequest(searchCriteria);
                var searchResponse = await _searchProvider.SearchAsync(searchCriteria.DocumentType, searchRequest);

                if (searchResponse != null)
                {
                    var returnProductsFromIndex = _settingsManager.GetValue("VirtoCommerce.SearchApi.UseFullObjectIndexStoring", true);

                    result.TotalCount = searchResponse.TotalCount;
                    result.Products = ConvertDocumentsToProducts(searchResponse.Documents, returnProductsFromIndex, searchCriteria, responseGroup);
                    result.Aggregations = ConvertFacetsToAggregations(searchResponse.Aggregations, searchCriteria);
                }
            }

            return result;
        }


        private static SearchRequest ConvertSearchCriteriaToSearchRequest(ProductSearchCriteria searchCriteria)
        {
            return null;
        }

        private static Product[] ConvertDocumentsToProducts(IList<SearchDocument> documents, bool returnProductsFromIndex, ProductSearchCriteria searchCriteria, ItemResponseGroup responseGroup)
        {
            return null;
        }

        private static Aggregation[] ConvertFacetsToAggregations(IList<AggregationResponse> aggregations, ProductSearchCriteria searchCriteria)
        {
            return null;
        }


        private IList<ISearchFilter> GetPredefinedFilters(Store store)
        {
            var context = new Dictionary<string, object>
            {
                { "Store", store },
            };

            return _browseFilterService.GetFilters(context);
        }
    }
}
