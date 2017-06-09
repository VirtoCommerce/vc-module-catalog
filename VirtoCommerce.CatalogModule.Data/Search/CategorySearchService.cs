using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Category = VirtoCommerce.CatalogModule.Web.Model.Category;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchService : ICategorySearchService
    {
        private readonly IStoreService _storeService;
        private readonly ISearchProvider _searchProvider;
        private readonly ISearchRequestBuilder[] _searchRequestBuilders;
        private readonly ISettingsManager _settingsManager;

        public CategorySearchService(IStoreService storeService, ISearchProvider searchProvider, ISearchRequestBuilder[] searchRequestBuilders, ISettingsManager settingsManager)
        {
            _storeService = storeService;
            _searchProvider = searchProvider;
            _searchRequestBuilders = searchRequestBuilders;
            _settingsManager = settingsManager;
        }

        public async Task<CategorySearchResult> SearchCategoriesAsync(string storeId, CategorySearch categorySearch)
        {
            var result = new CategorySearchResult();

            var store = _storeService.GetById(storeId);
            if (store != null)
            {
                var responseGroup = EnumUtility.SafeParse(categorySearch.ResponseGroup, CategoryResponseGroup.Full & ~CategoryResponseGroup.WithProperties);
                var searchCriteria = categorySearch.AsCriteria<CategorySearchCriteria>(store.Catalog);

                var searchRequest = ConvertSearchCriteriaToSearchRequest(searchCriteria);
                var searchResponse = await _searchProvider.SearchAsync(searchCriteria.DocumentType, searchRequest);

                if (searchResponse != null)
                {
                    var returnProductsFromIndex = _settingsManager.GetValue("VirtoCommerce.SearchApi.UseFullObjectIndexStoring", true);

                    result.TotalCount = searchResponse.TotalCount;
                    result.Categories = ConvertDocumentsToCategories(searchResponse.Documents, returnProductsFromIndex, searchCriteria, responseGroup);
                }
            }

            return result;
        }

        private static SearchRequest ConvertSearchCriteriaToSearchRequest(CategorySearchCriteria searchCriteria)
        {
            return null;
        }

        private static Category[] ConvertDocumentsToCategories(IList<SearchDocument> documents, bool returnProductsFromIndex, CategorySearchCriteria searchCriteria, CategoryResponseGroup responseGroup)
        {
            return null;
        }
    }
}
