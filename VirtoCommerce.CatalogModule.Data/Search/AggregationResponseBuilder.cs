using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using RangeFilter = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilter;
using RangeFilterValue = VirtoCommerce.CatalogModule.Data.Search.BrowseFilters.RangeFilterValue;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class AggregationResponseBuilder : IAggregationResponseBuilder
    {
        private readonly IBrowseFilterService _browseFilterService;
        private readonly IAggregationLabelService _aggregationLabelService;

        public AggregationResponseBuilder(IBrowseFilterService browseFilterService, IAggregationLabelService aggregationLabelService)
        {
            _browseFilterService = browseFilterService;
            _aggregationLabelService = aggregationLabelService;
        }

        public virtual Aggregation[] ConvertAggregations(IList<AggregationResponse> aggregationResponses, ProductSearchCriteria criteria)
        {
            var result = new List<Aggregation>();

            var browseFilters = _browseFilterService.GetBrowseFilters(criteria);
            if (browseFilters != null && aggregationResponses?.Any() == true)
            {
                foreach (var filter in browseFilters)
                {
                    Aggregation aggregation = null;

                    var attributeFilter = filter as AttributeFilter;
                    var rangeFilter = filter as RangeFilter;
                    var priceRangeFilter = filter as PriceRangeFilter;

                    if (attributeFilter != null)
                    {
                        aggregation = GetAttributeAggregation(attributeFilter, aggregationResponses);
                    }
                    else if (rangeFilter != null)
                    {
                        aggregation = GetRangeAggregation(rangeFilter, aggregationResponses);
                    }
                    else if (priceRangeFilter != null)
                    {
                        aggregation = GetPriceRangeAggregation(priceRangeFilter, aggregationResponses);
                    }

                    if (aggregation?.Items?.Any() == true)
                    {
                        result.Add(aggregation);
                    }
                }
            }

            // Add localized labels for names and values
            if (result.Any())
            {
                _aggregationLabelService.AddLabels(result, criteria.CatalogId);
            }

            return result.ToArray();
        }

        protected virtual Aggregation GetAttributeAggregation(AttributeFilter attributeFilter, IList<AggregationResponse> aggregationResponses)
        {
            Aggregation result = null;

            var fieldName = attributeFilter.Key;
            var aggregationResponse = aggregationResponses.FirstOrDefault(a => a.Id.EqualsInvariant(fieldName));

            if (aggregationResponse != null)
            {
                IList<AggregationResponseValue> aggregationResponseValues;

                if (attributeFilter.Values == null)
                {
                    // Return all values
                    aggregationResponseValues = aggregationResponse.Values;
                }
                else
                {
                    // Return predefined values
                    aggregationResponseValues = attributeFilter.Values
                        .GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase)
                        .Select(g => aggregationResponse.Values.FirstOrDefault(v => v.Id.EqualsInvariant(g.Key)))
                        .Where(v => v != null)
                        .ToArray();
                }

                if (aggregationResponseValues.Any())
                {
                    result = new Aggregation
                    {
                        AggregationType = "attr",
                        Field = fieldName,
                        Items = GetAttributeAggregationItems(aggregationResponseValues),
                    };
                }
            }

            return result;
        }

        protected virtual Aggregation GetRangeAggregation(RangeFilter rangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var result = new Aggregation
            {
                AggregationType = "range",
                Field = rangeFilter.Key,
                Items = GetRangeAggregationItems(rangeFilter.Key, rangeFilter.Values, aggregationResponses),
            };

            return result;
        }

        protected virtual Aggregation GetPriceRangeAggregation(PriceRangeFilter priceRangeFilter, IList<AggregationResponse> aggregationResponses)
        {
            var result = new Aggregation
            {
                AggregationType = "pricerange",
                Field = priceRangeFilter.Key,
                Items = GetRangeAggregationItems(priceRangeFilter.Key, priceRangeFilter.Values, aggregationResponses),
            };


            return result;
        }

        protected virtual IList<AggregationItem> GetAttributeAggregationItems(IList<AggregationResponseValue> aggregationResponseValues)
        {
            var result = aggregationResponseValues
                .Select(v =>
                    new AggregationItem
                    {
                        Value = v.Id,
                        Count = (int)v.Count,
                    })
                .ToList();

            return result;
        }

        protected virtual IList<AggregationItem> GetRangeAggregationItems(string aggregationId, IList<RangeFilterValue> values, IList<AggregationResponse> aggregationResponses)
        {
            var result = new List<AggregationItem>();

            if (values != null)
            {
                foreach (var group in values.GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase))
                {
                    var valueId = group.Key;
                    var aggregationValueId = $"{aggregationId}-{valueId}";
                    var aggregationResponse = aggregationResponses.FirstOrDefault(v => v.Id.EqualsInvariant(aggregationValueId));

                    if (aggregationResponse?.Values?.Any() == true)
                    {
                        var value = aggregationResponse.Values.First();
                        var rangeValue = group.First();

                        var aggregationItem = new AggregationItem
                        {
                            Value = valueId,
                            Count = (int)value.Count,
                            RequestedLowerBound = !string.IsNullOrEmpty(rangeValue.Lower) ? rangeValue.Lower : null,
                            RequestedUpperBound = !string.IsNullOrEmpty(rangeValue.Upper) ? rangeValue.Upper : null,
                        };

                        result.Add(aggregationItem);
                    }
                }
            }

            return result;
        }
    }
}
