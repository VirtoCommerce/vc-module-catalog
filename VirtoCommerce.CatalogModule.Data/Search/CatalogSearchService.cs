using System;
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
    public abstract class CatalogSearchService<TSearch, TItem, TResult>
        where TItem : Entity
        where TResult : CatalogSearchResult<TItem>
    {
        private readonly IStoreService _storeService;
        private readonly ISearchCriteriaPreprocessor[] _searchCriteriaPreprocessors;
        private readonly ISearchRequestBuilder[] _searchRequestBuilders;
        private readonly ISearchProvider _searchProvider;
        private readonly ISettingsManager _settingsManager;

        protected CatalogSearchService(IStoreService storeService, ISearchCriteriaPreprocessor[] searchCriteriaPreprocessors, ISearchRequestBuilder[] searchRequestBuilders, ISearchProvider searchProvider, ISettingsManager settingsManager)
        {
            _storeService = storeService;
            _searchCriteriaPreprocessors = searchCriteriaPreprocessors;
            _searchRequestBuilders = searchRequestBuilders;
            _searchProvider = searchProvider;
            _settingsManager = settingsManager;
        }

        public virtual async Task<TResult> SearchAsync(string storeId, TSearch search)
        {
            var result = AbstractTypeFactory<TResult>.TryCreateInstance();

            var store = _storeService.GetById(storeId);
            if (store != null)
            {
                var criteria = GetSearchCriteria(search, store);

                _searchCriteriaPreprocessors.ForEach(p => p.Process(criteria));

                var requestBuilder = GetRequestBuilder(criteria);
                var request = requestBuilder?.BuildRequest(criteria);

                var response = await _searchProvider.SearchAsync(criteria.DocumentType, request);

                if (response != null)
                {
                    result.TotalCount = response.TotalCount;
                    result.Items = ConvertDocuments(response.Documents, criteria, search);
                    result.Aggregations = ConvertAggregations(response.Aggregations, criteria);
                }
            }

            return result;
        }


        protected abstract SearchCriteria GetSearchCriteria(TSearch search, Store store);
        protected abstract IList<TItem> LoadMissingItems(string[] missingItemIds, SearchCriteria criteria, TSearch search);
        protected abstract void ReduceSearchResults(IEnumerable<TItem> items, TSearch search);


        protected virtual ISearchRequestBuilder GetRequestBuilder(SearchCriteria criteria)
        {
            if (_searchRequestBuilders == null)
                throw new InvalidOperationException("No query builders defined");

            var queryBuilder = _searchRequestBuilders.SingleOrDefault(b => b.DocumentType.Equals(criteria.DocumentType)) ??
                               _searchRequestBuilders.First(b => b.DocumentType.Equals(string.Empty));

            return queryBuilder;
        }

        protected virtual TItem[] ConvertDocuments(IList<SearchDocument> documents, SearchCriteria criteria, TSearch search)
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
                    var missingItems = LoadMissingItems(missingObjectIds, criteria, search);

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

        protected virtual Aggregation[] ConvertAggregations(IList<AggregationResponse> aggregationResponses, SearchCriteria criteria)
        {
            return null;
        }
    }
}
