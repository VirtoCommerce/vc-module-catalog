using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using RangeFilter = VirtoCommerce.Domain.Search.RangeFilter;
using RangeFilterValue = VirtoCommerce.Domain.Search.RangeFilterValue;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchRequestBuilder : CatalogSearchRequestBuilder
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Product;

        public override SearchRequest BuildRequest(SearchCriteria criteria)
        {
            var request = base.BuildRequest(criteria);

            var productSearchCriteria = criteria as ProductSearchCriteria;
            if (productSearchCriteria != null)
            {
                request.Aggregations = GetAggregationRequests(productSearchCriteria);
            }

            return request;
        }

        protected override IList<IFilter> GetFilters(SearchCriteria criteria)
        {
            return GetFiltersExceptSpecified(criteria, null);
        }


        private IList<IFilter> GetFiltersExceptSpecified(SearchCriteria criteria, string excludeFieldName)
        {
            var result = base.GetFilters(criteria);

            var productSearchCriteria = criteria as ProductSearchCriteria;
            if (productSearchCriteria?.CurrentFilters?.Any() == true)
            {
                result.AddRange(productSearchCriteria.CurrentFilters
                    .Where(f => !f.Key.EqualsInvariant(excludeFieldName))
                    .Select(f => ConvertFilter(f, productSearchCriteria))
                    .Where(f => f != null));
            }

            return result;
        }

        private static IFilter ConvertFilter(IBrowseFilter filter, ProductSearchCriteria criteria)
        {
            IFilter result = null;

            var attributeFilter = filter as AttributeFilter;
            var rangeFilter = filter as BrowseFilters.RangeFilter;
            var priceRangeFilter = filter as PriceRangeFilter;

            if (attributeFilter != null)
            {
                result = ConvertAttributeFilter(attributeFilter);
            }
            else if (rangeFilter != null)
            {
                result = ConvertRangeFilter(rangeFilter);
            }
            else if (priceRangeFilter != null)
            {
                result = ConvertPriceRangeFilter(priceRangeFilter, criteria);
            }

            return result;
        }

        private static IFilter ConvertAttributeFilter(AttributeFilter attributeFilter)
        {
            var result = new TermFilter
            {
                FieldName = attributeFilter.Key,
                Values = attributeFilter.Values?.Select(v => v.Value).ToArray(),
            };

            return result;
        }

        private static IFilter ConvertRangeFilter(BrowseFilters.RangeFilter rangeFilter)
        {
            var result = new RangeFilter
            {
                FieldName = rangeFilter.Key,
                Values = rangeFilter.Values?.Select(ConvertRangeFilterValue).ToArray(),
            };

            return result;
        }

        private static RangeFilterValue ConvertRangeFilterValue(BrowseFilters.RangeFilterValue rangeFilterValue)
        {
            return new RangeFilterValue
            {
                Lower = rangeFilterValue.Lower,
                Upper = rangeFilterValue.Upper,
                IncludeLower = rangeFilterValue.IncludeLower,
                IncludeUpper = rangeFilterValue.IncludeUpper,
            };
        }

        private static IFilter ConvertPriceRangeFilter(PriceRangeFilter priceRangeFilter, ProductSearchCriteria criteria)
        {
            IFilter result = null;

            if (string.IsNullOrEmpty(criteria.Currency) || priceRangeFilter.Currency.EqualsInvariant(criteria.Currency))
            {
                var filters = priceRangeFilter.Values
                    .Select(v => FiltersHelper.CreatePriceRangeFilter(priceRangeFilter.Currency, criteria.Pricelists, v.Lower, v.Upper, v.IncludeLower, v.IncludeUpper))
                    .Where(f => f != null)
                    .ToList();

                result = filters.Or();
            }

            return result;
        }

        private IList<AggregationRequest> GetAggregationRequests(ProductSearchCriteria criteria)
        {
            var result = new List<AggregationRequest>();

            if (criteria.BrowseFilters != null)
            {
                foreach (var filter in criteria.BrowseFilters)
                {
                    var existingFilters = GetFiltersExceptSpecified(criteria, filter.Key);

                    var attributeFilter = filter as AttributeFilter;
                    var priceRangeFilter = filter as PriceRangeFilter;
                    var rangeFilter = filter as BrowseFilters.RangeFilter;

                    AggregationRequest aggregationRequest = null;
                    IList<AggregationRequest> aggregationRequests = null;

                    if (attributeFilter != null)
                    {
                        aggregationRequest = GetAttributeFilterAggregationRequest(attributeFilter, existingFilters);
                    }
                    else if (rangeFilter != null)
                    {
                        aggregationRequests = GetRangeFilterAggregationRequests(rangeFilter, existingFilters);
                    }
                    else if (priceRangeFilter != null && priceRangeFilter.Currency.EqualsInvariant(criteria.Currency))
                    {
                        aggregationRequests = GetPriceRangeFilterAggregationRequests(priceRangeFilter, criteria, existingFilters);
                    }

                    if (aggregationRequest != null)
                    {
                        result.Add(aggregationRequest);
                    }

                    if (aggregationRequests != null)
                    {
                        result.AddRange(aggregationRequests.Where(f => f != null));
                    }
                }
            }

            return result;
        }

        private static AggregationRequest GetAttributeFilterAggregationRequest(AttributeFilter attributeFilter, IEnumerable<IFilter> existingFilters)
        {
            return new TermAggregationRequest
            {
                FieldName = attributeFilter.Key,
                Values = attributeFilter.Values?.Select(v => v.Id).ToArray(),
                Filter = existingFilters.And(),
            };
        }

        private static IList<AggregationRequest> GetRangeFilterAggregationRequests(BrowseFilters.RangeFilter rangeFilter, IList<IFilter> existingFilters)
        {
            var result = rangeFilter.Values.Select(v => GetRangeFilterValueAggregationRequest(rangeFilter.Key, v, existingFilters)).ToList();
            return result;
        }

        private static AggregationRequest GetRangeFilterValueAggregationRequest(string fieldName, BrowseFilters.RangeFilterValue value, IEnumerable<IFilter> existingFilters)
        {
            var valueFilter = FiltersHelper.CreateRangeFilter(fieldName, value.Lower, value.Upper, value.IncludeLower, value.IncludeUpper);

            var result = new RangeAggregationRequest
            {
                Id = $"{fieldName}-{value.Id}",
                Filter = existingFilters.And(valueFilter)
            };

            return result;
        }

        private static IList<AggregationRequest> GetPriceRangeFilterAggregationRequests(PriceRangeFilter priceRangeFilter, ProductSearchCriteria criteria, IList<IFilter> existingFilters)
        {
            var result = priceRangeFilter.Values.Select(v => GetPriceRangeFilterValueAggregationRequest(priceRangeFilter, v, existingFilters, criteria.Pricelists)).ToList();
            return result;
        }

        private static AggregationRequest GetPriceRangeFilterValueAggregationRequest(PriceRangeFilter priceRangeFilter, BrowseFilters.RangeFilterValue value, IEnumerable<IFilter> existingFilters, IList<string> pricelists)
        {
            var valueFilter = FiltersHelper.CreatePriceRangeFilter(priceRangeFilter.Currency, pricelists, value.Lower, value.Upper, value.IncludeLower, value.IncludeUpper);

            var result = new TermAggregationRequest
            {
                Id = $"{priceRangeFilter.Key}-{value.Id}",
                Filter = existingFilters.And(valueFilter)
            };

            return result;
        }
    }
}
