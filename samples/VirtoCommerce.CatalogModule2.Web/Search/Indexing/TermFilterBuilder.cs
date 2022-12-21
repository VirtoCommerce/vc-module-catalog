using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using RangeFilterValue = VirtoCommerce.SearchModule.Core.Model.RangeFilterValue;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class TermFilterBuilder2 : TermFilterBuilder
    {
        public TermFilterBuilder2(IBrowseFilterService browseFilterService, ISearchPhraseParser searchPhraseParser) : base(browseFilterService, searchPhraseParser)
        {
        }
        protected override IFilter ConvertAttributeFilter(AttributeFilter attributeFilter, IList<string> valueIds)
        {
            return base.ConvertAttributeFilter(attributeFilter, valueIds);
        }
        protected override IFilter ConvertBrowseFilter(IBrowseFilter filter, IList<string> valueIds, ProductIndexedSearchCriteria criteria)
        {
            return base.ConvertBrowseFilter(filter, valueIds, criteria);
        }
        protected override IFilter ConvertPriceRangeFilter(PriceRangeFilter priceRangeFilter, IList<string> valueIds, ProductIndexedSearchCriteria criteria)
        {
            return base.ConvertPriceRangeFilter(priceRangeFilter, valueIds, criteria);
        }
        protected override IFilter ConvertRangeFilter(CatalogModule.Data.Search.BrowseFilters.RangeFilter rangeFilter, IList<string> valueIds)
        {
            return base.ConvertRangeFilter(rangeFilter, valueIds);
        }
        protected override RangeFilterValue ConvertRangeFilterValue(CatalogModule.Data.Search.BrowseFilters.RangeFilterValue rangeFilterValue)
        {
            return base.ConvertRangeFilterValue(rangeFilterValue);
        }
        public override Task<FiltersContainer> GetTermFiltersAsync(ProductIndexedSearchCriteria criteria)
        {
            return base.GetTermFiltersAsync(criteria);
        }
    }
}
