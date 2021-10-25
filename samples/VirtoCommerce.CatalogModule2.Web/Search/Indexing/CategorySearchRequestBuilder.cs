using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class CategorySearchRequestBuilder2 : CategorySearchRequestBuilder
    {
        public CategorySearchRequestBuilder2(ISearchPhraseParser searchPhraseParser) : base(searchPhraseParser)
        {
        }
        public override Task<SearchRequest> BuildRequestAsync(SearchCriteriaBase criteria)
        {
            return base.BuildRequestAsync(criteria);
        }
        protected override IList<IFilter> GetFilters(CategoryIndexedSearchCriteria criteria)
        {
            return base.GetFilters(criteria);
        }
        protected override IList<SortingField> GetSorting(CategoryIndexedSearchCriteria criteria)
        {
            return base.GetSorting(criteria);
        }
    }
}
