using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using RangeFilter = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilter;
using RangeFilterValue = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilterValue;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class AggregationConverter2 : AggregationConverter
    {
        public AggregationConverter2(IBrowseFilterService browseFilterService, IPropertyService propertyService, IPropertyDictionaryItemSearchService propDictItemsSearchService) : base(browseFilterService, propertyService, propDictItemsSearchService)
        {
        }
        protected override Task AddLabelsAsync(IList<Aggregation> aggregations, string catalogId)
        {
            return base.AddLabelsAsync(aggregations, catalogId);
        }
        public override Task<Aggregation[]> ConvertAggregationsAsync(IList<AggregationResponse> aggregationResponses, ProductIndexedSearchCriteria criteria)
        {
            return base.ConvertAggregationsAsync(aggregationResponses, criteria);
        }
        protected override Aggregation GetAttributeAggregation(AttributeFilter attributeFilter, IList<AggregationResponse> aggregationResponses)
        {
            return base.GetAttributeAggregation(attributeFilter, aggregationResponses);
        }
        protected override IList<AggregationItem> GetAttributeAggregationItems(IList<AggregationResponseValue> aggregationResponseValues)
        {
            return base.GetAttributeAggregationItems(aggregationResponseValues);
        }
        protected override AggregationRequest GetAttributeFilterAggregationRequest(AttributeFilter attributeFilter, IEnumerable<IFilter> existingFilters)
        {
            return base.GetAttributeFilterAggregationRequest(attributeFilter, existingFilters);
        }
        protected override Aggregation GetPriceRangeAggregation(PriceRangeFilter priceRangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            return base.GetPriceRangeAggregation(priceRangeFilter, aggregationResponses);
        }
        protected override IList<AggregationRequest> GetPriceRangeFilterAggregationRequests(PriceRangeFilter priceRangeFilter, ProductIndexedSearchCriteria criteria, IList<IFilter> existingFilters)
        {
            return base.GetPriceRangeFilterAggregationRequests(priceRangeFilter, criteria, existingFilters);
        }
        protected override AggregationRequest GetPriceRangeFilterValueAggregationRequest(PriceRangeFilter priceRangeFilter, RangeFilterValue value, IEnumerable<IFilter> existingFilters, IList<string> pricelists)
        {
            return base.GetPriceRangeFilterValueAggregationRequest(priceRangeFilter, value, existingFilters, pricelists);
        }
        protected override Aggregation GetRangeAggregation(RangeFilter rangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            return base.GetRangeAggregation(rangeFilter, aggregationResponses);
        }
        protected override IList<AggregationItem> GetRangeAggregationItems(string aggregationId, IList<RangeFilterValue> values, IList<AggregationResponse> aggregationResponses)
        {
            return base.GetRangeAggregationItems(aggregationId, values, aggregationResponses);
        }
        protected override IList<AggregationRequest> GetRangeFilterAggregationRequests(RangeFilter rangeFilter, IList<IFilter> existingFilters)
        {
            return base.GetRangeFilterAggregationRequests(rangeFilter, existingFilters);
        }
        protected override AggregationRequest GetRangeFilterValueAggregationRequest(string fieldName, RangeFilterValue value, IEnumerable<IFilter> existingFilters)
        {
            return base.GetRangeFilterValueAggregationRequest(fieldName, value, existingFilters);
        }
    }
}
