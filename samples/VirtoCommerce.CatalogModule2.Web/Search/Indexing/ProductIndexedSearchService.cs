using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class ProductIndexedSearchService2 : ProductIndexedSearchService
    {
        public ProductIndexedSearchService2(ISearchRequestBuilderRegistrar searchRequestBuilderRegistrar, ISearchProvider searchProvider, ISettingsManager settingsManager, IItemService itemService, IBlobUrlResolver blobUrlResolver, IAggregationConverter aggregationConverter) : base(
            searchRequestBuilderRegistrar,
            searchProvider,
            settingsManager,
            itemService,
            blobUrlResolver,
            aggregationConverter
            )
        {
        }
        protected override Task<Aggregation[]> ConvertAggregationsAsync(IList<AggregationResponse> aggregationResponses, ProductIndexedSearchCriteria criteria)
        {
            return base.ConvertAggregationsAsync(aggregationResponses, criteria);
        }
        protected override Task<CatalogProduct[]> ConvertDocuments(IList<SearchDocument> documents, ProductIndexedSearchCriteria criteria)
        {
            return base.ConvertDocuments(documents, criteria);
        }
        protected override ISearchRequestBuilder GetRequestBuilder(ProductIndexedSearchCriteria criteria)
        {
            return base.GetRequestBuilder(criteria);
        }
        protected override ItemResponseGroup GetResponseGroup(ProductIndexedSearchCriteria criteria)
        {
            return base.GetResponseGroup(criteria);
        }
        protected override Task<IList<CatalogProduct>> LoadMissingItems(string[] missingItemIds, ProductIndexedSearchCriteria criteria)
        {
            return base.LoadMissingItems(missingItemIds, criteria);
        }
        protected override void ReduceSearchResults(IEnumerable<CatalogProduct> products, ProductIndexedSearchCriteria criteria)
        {
            base.ReduceSearchResults(products, criteria);
        }
        public override Task<ProductIndexedSearchResult> SearchAsync(ProductIndexedSearchCriteria criteria)
        {
            return base.SearchAsync(criteria);
        }
    }
}
