using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using SearchCriteria = VirtoCommerce.Domain.Search.SearchCriteria;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public abstract class BaseSearchService<TSearch, TItem, TResult>
        where TItem : Entity
        where TResult : BaseSearchResult<TItem>
    {
        private readonly ISearchProvider _searchProvider;
        private readonly ISearchRequestBuilder[] _searchRequestBuilders;
        private readonly IStoreService _storeService;
        private readonly ISettingsManager _settingsManager;

        protected BaseSearchService(ISearchProvider searchProvider, ISearchRequestBuilder[] searchRequestBuilders, IStoreService storeService, ISettingsManager settingsManager)
        {
            _searchProvider = searchProvider;
            _searchRequestBuilders = searchRequestBuilders;
            _settingsManager = settingsManager;
            _storeService = storeService;
        }

        public virtual async Task<TResult> SearchAsync(string storeId, TSearch search)
        {
            var result = AbstractTypeFactory<TResult>.TryCreateInstance();

            var store = _storeService.GetById(storeId);
            if (store != null)
            {
                var searchCriteria = GetSearchCriteria(search, store);
                var searchRequest = ConvertSearchCriteriaToSearchRequest(searchCriteria);

                var searchResponse = await _searchProvider.SearchAsync(searchCriteria.DocumentType, searchRequest);

                ConvertSearchResults(result, searchResponse, searchCriteria, search);
            }

            return result;
        }


        protected abstract SearchCriteria GetSearchCriteria(TSearch search, Store store);
        protected abstract IList<TItem> LoadMissingItems(string[] missingItemIds, SearchCriteria searchCriteria, TSearch search);
        protected abstract void ReduceSearchResults(IEnumerable<TItem> items, TSearch search);


        protected SearchRequest ConvertSearchCriteriaToSearchRequest(SearchCriteria searchCriteria)
        {
            var request = new SearchRequest();

            var requestBuilders = _searchRequestBuilders
                .Where(b => b.DocumentType.EqualsInvariant(searchCriteria?.DocumentType))
                .ToList();

            foreach (var requestBuilder in requestBuilders)
            {
                requestBuilder.BuildRequest(request, searchCriteria);
            }

            return request;
        }

        protected virtual void ConvertSearchResults(TResult result, SearchResponse searchResponse, SearchCriteria searchCriteria, TSearch search)
        {
            if (searchResponse != null)
            {
                result.TotalCount = searchResponse.TotalCount;
                result.Items = ConvertDocuments(searchResponse.Documents, searchCriteria, search);
                result.Aggregations = ConvertAggregations(searchResponse.Aggregations, searchCriteria);
            }
        }

        protected virtual TItem[] ConvertDocuments(IList<SearchDocument> documents, SearchCriteria searchCriteria, TSearch search)
        {
            TItem[] result = null;

            if (documents?.Any() == true)
            {
                var returnObjectsFromIndex = _settingsManager.GetValue("VirtoCommerce.SearchApi.UseFullObjectIndexStoring", true);
                var itemsMap = documents.ToDictionary(doc => doc.Id.ToString(), doc => returnObjectsFromIndex ? doc.GetObjectFieldValue<TItem>() : null);

                var missingObjectIds = itemsMap
                    .Where(kvp => kvp.Value == null)
                    .Select(kvp => kvp.Key)
                    .ToArray();

                if (missingObjectIds.Any())
                {
                    var missingItems = LoadMissingItems(missingObjectIds, searchCriteria, search);

                    foreach (var item in missingItems)
                    {
                        itemsMap[item.Id] = item;
                    }
                }

                ReduceSearchResults(itemsMap.Values.Where(v => v != null), search);

                // Preserve original sorting order
                result = documents.Select(doc => itemsMap[doc.Id.ToString()]).ToArray();
            }

            return result;
        }

        protected virtual Aggregation[] ConvertAggregations(IList<AggregationResponse> aggregations, SearchCriteria searchCriteria)
        {
            return null;
        }
    }
}
