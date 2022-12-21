using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class CategoryIndexedSearchService2 : CategoryIndexedSearchService
    {
        public CategoryIndexedSearchService2(ISearchRequestBuilderRegistrar searchRequestBuilderRegistrar, ISearchProvider searchProvider, ISettingsManager settingsManager, ICategoryService categoryService)
            : base(searchRequestBuilderRegistrar, searchProvider, settingsManager, categoryService)
        {
        }

        protected override Task<Aggregation[]> ConvertAggregationsAsync(IList<AggregationResponse> aggregationResponses, CategoryIndexedSearchCriteria criteria)
        {
            return base.ConvertAggregationsAsync(aggregationResponses, criteria);
        }
        protected override Task<Category[]> ConvertDocuments(IList<SearchDocument> documents, CategoryIndexedSearchCriteria criteria)
        {
            return base.ConvertDocuments(documents, criteria);
        }
        protected override ISearchRequestBuilder GetRequestBuilder(CategoryIndexedSearchCriteria criteria)
        {
            return base.GetRequestBuilder(criteria);
        }
        protected override CategoryResponseGroup GetResponseGroup(CategoryIndexedSearchCriteria criteria)
        {
            return base.GetResponseGroup(criteria);
        }
        protected override Task<IList<Category>> LoadMissingItems(string[] missingItemIds, CategoryIndexedSearchCriteria criteria)
        {
            return base.LoadMissingItems(missingItemIds, criteria);
        }
        protected override void ReduceSearchResults(IEnumerable<Category> items, CategoryIndexedSearchCriteria criteria)
        {
            base.ReduceSearchResults(items, criteria);
        }
        public override Task<CategoryIndexedSearchResult> SearchAsync(CategoryIndexedSearchCriteria criteria)
        {
            return base.SearchAsync(criteria);
        }
    }
}
