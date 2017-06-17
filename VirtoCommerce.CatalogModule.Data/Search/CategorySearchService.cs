using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Category = VirtoCommerce.CatalogModule.Web.Model.Category;
using SearchCriteria = VirtoCommerce.Domain.Search.SearchCriteria;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CategorySearchService : CatalogSearchService<CategorySearch, Category, CategorySearchResult>, ICategorySearchService
    {
        private readonly ICategoryService _categoryService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public CategorySearchService(IStoreService storeService, ISearchCriteriaPreprocessor[] searchCriteriaPreprocessors, ISearchRequestBuilder[] searchRequestBuilders, ISearchProvider searchProvider, ISettingsManager settingsManager, ICategoryService categoryService, IBlobUrlResolver blobUrlResolver)
            : base(storeService, searchCriteriaPreprocessors, searchRequestBuilders, searchProvider, settingsManager)
        {
            _categoryService = categoryService;
            _blobUrlResolver = blobUrlResolver;
        }


        protected override SearchCriteria GetSearchCriteria(CategorySearch search, Store store)
        {
            var result = search.AsCriteria<CategorySearchCriteria>(store.Catalog);
            return result;
        }

        protected override IList<Category> LoadMissingItems(string[] missingItemIds, SearchCriteria criteria, CategorySearch search)
        {
            var catalog = (criteria as CategorySearchCriteria)?.Catalog;
            var categories = _categoryService.GetByIds(missingItemIds, GetResponseGroup(search), catalog);
            var result = categories.Select(p => p.ToWebModel(_blobUrlResolver)).ToArray();
            return result;
        }

        protected override void ReduceSearchResults(IEnumerable<Category> items, CategorySearch search)
        {
        }


        protected virtual CategoryResponseGroup GetResponseGroup(CategorySearch search)
        {
            var result = EnumUtility.SafeParse(search.ResponseGroup, CategoryResponseGroup.Full & ~CategoryResponseGroup.WithProperties);
            return result;
        }
    }
}
