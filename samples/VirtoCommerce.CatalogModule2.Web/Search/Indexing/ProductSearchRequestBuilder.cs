using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class ProductSearchRequestBuilder2 : ProductSearchRequestBuilder
    {
        public ProductSearchRequestBuilder2(ISearchPhraseParser searchPhraseParser, ITermFilterBuilder termFilterBuilder, IAggregationConverter aggregationConverter) : base(searchPhraseParser, termFilterBuilder, aggregationConverter)
        {
        }
        public override Task<SearchRequest> BuildRequestAsync(SearchCriteriaBase criteria)
        {
            return base.BuildRequestAsync(criteria);
        }
        protected override Task<FiltersContainer> GetAllFiltersAsync(ProductIndexedSearchCriteria criteria)
        {
            return base.GetAllFiltersAsync(criteria);
        }
        protected override IList<IFilter> GetFilters(FiltersContainer allFilters)
        {
            return base.GetFilters(allFilters);
        }
        protected override IList<IFilter> GetPermanentFilters(ProductIndexedSearchCriteria criteria)
        {
            return base.GetPermanentFilters(criteria);
        }
        protected override IList<SortingField> GetSorting(ProductIndexedSearchCriteria criteria)
        {
            return base.GetSorting(criteria);
        }
    }
}
