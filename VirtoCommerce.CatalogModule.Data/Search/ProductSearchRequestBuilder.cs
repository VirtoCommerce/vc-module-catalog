using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using RangeFilter = VirtoCommerce.Domain.Search.RangeFilter;
using RangeFilterValue = VirtoCommerce.Domain.Search.RangeFilterValue;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchRequestBuilder : CatalogSearchRequestBuilder
    {
        public override string DocumentType { get; } = KnownDocumentTypes.Product;

        public override void BuildRequest(SearchRequest request, SearchCriteria criteria)
        {
            base.BuildRequest(request, criteria);

            var productSearchCriteria = criteria as ProductSearchCriteria;
            if (productSearchCriteria != null)
            {
                request.Aggregations = GetAggregations(productSearchCriteria);
            }
        }

        protected override IList<IFilter> GetFilters(SearchCriteria criteria)
        {
            return GetFilters(criteria, null);
        }


        private IList<IFilter> GetFilters(SearchCriteria criteria, string excludeFieldName)
        {
            var result = base.GetFilters(criteria);

            var productSearchCriteria = criteria as ProductSearchCriteria;
            if (productSearchCriteria != null)
            {
                var productFilters = GetProductFilters(productSearchCriteria);

                if (productFilters?.Any() == true)
                {
                    result.AddRange(productFilters
                        .Where(f => !f.Key.EqualsInvariant(excludeFieldName))
                        .Select(f => ConvertFilter(f, productSearchCriteria))
                        .Where(f => f != null));
                }
            }

            return result;
        }

        private static IList<ISearchFilter> GetProductFilters(ProductSearchCriteria criteria)
        {
            var result = new List<ISearchFilter>
            {
                CreateCatalogDateRangeFilter("startdate", criteria.StartDateFrom, criteria.StartDate, false, true)
            };

            if (criteria.EndDate != null)
            {
                result.Add(CreateCatalogDateRangeFilter("enddate", criteria.EndDate, null, false, false));
            }

            if (!criteria.ClassTypes.IsNullOrEmpty())
            {
                result.Add(CreateCatalogAttributeFilter("__type", criteria.ClassTypes));
            }

            if (!string.IsNullOrEmpty(criteria.Catalog))
            {
                result.Add(CreateCatalogAttributeFilter("catalog", criteria.Catalog.ToLowerInvariant()));
            }

            if (!criteria.Outlines.IsNullOrEmpty())
            {
                var outlines = criteria.Outlines.Select(o => o.TrimEnd('/', '*').ToLowerInvariant());
                result.Add(CreateCatalogAttributeFilter("__outline", outlines));
            }

            if (!criteria.WithHidden)
            {
                result.Add(CreateCatalogAttributeFilter("status", "visible"));
            }

            if (criteria.PriceRange != null)
            {
                var range = criteria.PriceRange;
                result.Add(CreateCatalogPriceRangeFilter(criteria.Currency, range.Lower, range.Upper, range.IncludeLower, range.IncludeUpper));
            }

            if (criteria.CurrentFilters != null)
            {
                result.AddRange(criteria.CurrentFilters);
            }

            return result;
        }

        private static ISearchFilter CreateCatalogAttributeFilter(string key, string value)
        {
            return new AttributeFilter
            {
                Key = key,
                Values = new[] { new AttributeFilterValue { Value = value } },
            };
        }

        private static ISearchFilter CreateCatalogAttributeFilter(string key, IEnumerable<string> values)
        {
            return new AttributeFilter
            {
                Key = key,
                Values = values.Select(v => new AttributeFilterValue { Value = v }).ToArray(),
            };
        }

        private static ISearchFilter CreateCatalogDateRangeFilter(string key, DateTime? lower, DateTime? upper, bool includeLower, bool includeUpper)
        {
            return CreateCatalogRangeFilter(key, lower?.ToString("O"), upper?.ToString("O"), includeLower, includeUpper);
        }

        private static ISearchFilter CreateCatalogPriceRangeFilter(string currency, decimal? lower, decimal? upper, bool includeLower, bool includeUpper)
        {
            return new PriceRangeFilter
            {
                Currency = currency,
                Values = new[] { CreateCatalogRangeFilterValue(lower?.ToString(CultureInfo.InvariantCulture), upper?.ToString(CultureInfo.InvariantCulture), includeLower, includeUpper) },
            };
        }

        private static ISearchFilter CreateCatalogRangeFilter(string key, string lower, string upper, bool includeLower, bool includeUpper)
        {
            return new Filtering.RangeFilter
            {
                Key = key,
                Values = new[] { CreateCatalogRangeFilterValue(lower, upper, includeLower, includeUpper) },
            };
        }

        private static Filtering.RangeFilterValue CreateCatalogRangeFilterValue(string lower, string upper, bool includeLower, bool includeUpper)
        {
            return new Filtering.RangeFilterValue
            {
                Lower = lower,
                Upper = upper,
                IncludeLower = includeLower,
                IncludeUpper = includeUpper,
            };
        }

        private static IFilter ConvertFilter(ISearchFilter filter, ProductSearchCriteria criteria)
        {
            IFilter result = null;

            var attributeFilter = filter as AttributeFilter;
            var rangeFilter = filter as Filtering.RangeFilter;
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

        private static IFilter ConvertRangeFilter(Filtering.RangeFilter rangeFilter)
        {
            var result = new RangeFilter
            {
                FieldName = rangeFilter.Key,
                Values = rangeFilter.Values?.Select(ConvertRangeFilterValue).ToArray(),
            };

            return result;
        }

        private static RangeFilterValue ConvertRangeFilterValue(Filtering.RangeFilterValue rangeFilterValue)
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
                var commonFieldName = $"{priceRangeFilter.Key}_{priceRangeFilter.Currency}".ToLowerInvariant();

                var filters = priceRangeFilter.Values
                    .Select(v => GetPriceRangeFilterRecursive(0, criteria.Pricelists, commonFieldName, v.Lower, v.Upper, v.IncludeLower, v.IncludeUpper))
                    .Where(f => f != null)
                    .ToList();

                result = filters.Count > 1 ? new OrFilter { ChildFilters = filters } : filters.FirstOrDefault();
            }

            return result;
        }

        private static IFilter GetPriceRangeFilterRecursive(int pricelistNumber, IList<string> pricelists, string commonFieldName, string lower, string upper, bool includeLower, bool includeUpper)
        {
            IFilter result = null;

            if (pricelists.IsNullOrEmpty())
            {
                result = CreateRangeFilter(commonFieldName, lower, upper, includeLower, includeUpper);
            }
            else if (pricelistNumber < pricelists.Count)
            {
                // Create negative query for previous pricelist
                IFilter previousPricelistQuery = null;
                if (pricelistNumber > 0)
                {
                    var previousFieldName = $"{commonFieldName}_{pricelists[pricelistNumber - 1]}".ToLowerInvariant();
                    previousPricelistQuery = CreateRangeFilter(previousFieldName, "0", null, false, false);
                }

                // Create positive query for current pricelist
                var currentFieldName = $"{commonFieldName}_{pricelists[pricelistNumber]}".ToLowerInvariant();
                var currentPricelistQuery = CreateRangeFilter(currentFieldName, lower, upper, includeLower, includeUpper);

                // Get query for next pricelist
                var nextPricelistQuery = GetPriceRangeFilterRecursive(pricelistNumber + 1, pricelists, commonFieldName, lower, upper, includeLower, includeUpper);

                result = previousPricelistQuery.Not().And(currentPricelistQuery.Or(nextPricelistQuery));
            }

            return result;
        }

        private static IFilter CreateRangeFilter(string fieldName, string lower, string upper, bool includeLower, bool includeUpper)
        {
            return new RangeFilter
            {
                FieldName = fieldName,
                Values = new[]
                {
                    new RangeFilterValue
                    {
                        Lower = lower,
                        Upper = upper,
                        IncludeLower = includeLower,
                        IncludeUpper = includeUpper,
                    }
                },
            };
        }

        private IList<AggregationRequest> GetAggregations(ProductSearchCriteria criteria)
        {
            var result = new List<AggregationRequest>();

            foreach (var filter in criteria.Filters)
            {
                var attributeFilter = filter as AttributeFilter;
                var priceRangeFilter = filter as PriceRangeFilter;
                var rangeFilter = filter as Filtering.RangeFilter;

                AggregationRequest aggregationRequest = null;
                IList<AggregationRequest> aggregationRequests = null;

                if (attributeFilter != null)
                {
                    aggregationRequest = GetAttributeFilterAggregationRequest(attributeFilter, criteria);
                }
                else if (rangeFilter != null)
                {
                    aggregationRequests = GetRangeFilterAggregationRequests(rangeFilter, criteria);
                }
                else if (priceRangeFilter != null && priceRangeFilter.Currency.EqualsInvariant(criteria.Currency))
                {
                    aggregationRequests = GetPriceRangeFilterAggregationRequests(priceRangeFilter, criteria);
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

            return result;
        }

        private AggregationRequest GetAttributeFilterAggregationRequest(AttributeFilter attributeFilter, ProductSearchCriteria criteria)
        {
            return new TermAggregationRequest
            {
                FieldName = attributeFilter.Key,
                //Values = attributeFilter.Values?.Select(v => v.Id).ToArray(),
                Filter = GetFilters(criteria, attributeFilter.Key).And(),
            };
        }

        private IList<AggregationRequest> GetRangeFilterAggregationRequests(Filtering.RangeFilter rangeFilter, ProductSearchCriteria criteria)
        {
            var fieldName = rangeFilter.Key;
            var existingFilters = GetFilters(criteria, fieldName);

            var result = rangeFilter.Values.Select(v => GetRangeFilterValueAggregationRequest(fieldName, v, existingFilters)).ToList();
            return result;
        }

        private static AggregationRequest GetRangeFilterValueAggregationRequest(string fieldName, Filtering.RangeFilterValue value, IEnumerable<IFilter> existingFilters)
        {
            var valueFilter = CreateRangeFilter(fieldName, value.Lower, value.Upper, value.IncludeLower, value.IncludeUpper);

            var result = new RangeAggregationRequest
            {
                Id = $"{fieldName}-{value.Id}",
                Filter = existingFilters.And(valueFilter)
            };

            return result;
        }

        private IList<AggregationRequest> GetPriceRangeFilterAggregationRequests(PriceRangeFilter priceRangeFilter, ProductSearchCriteria criteria)
        {
            var fieldName = priceRangeFilter.Key;
            var existingFilters = GetFilters(criteria, fieldName);

            var result = priceRangeFilter.Values.Select(v => GetPriceRangeFilterValueAggregationRequest(priceRangeFilter, v, existingFilters, criteria.Pricelists)).ToList();
            return result;
        }

        private static AggregationRequest GetPriceRangeFilterValueAggregationRequest(PriceRangeFilter priceRangeFilter, Filtering.RangeFilterValue value, IEnumerable<IFilter> existingFilters, IList<string> pricelists)
        {
            var commonFieldName = $"{priceRangeFilter.Key}_{priceRangeFilter.Currency}".ToLowerInvariant();
            var valueFilter = GetPriceRangeFilterRecursive(0, pricelists, commonFieldName, value.Lower, value.Upper, value.IncludeLower, value.IncludeUpper);

            var result = new TermAggregationRequest
            {
                Id = $"{priceRangeFilter.Key}-{value.Id}",
                Filter = existingFilters.And(valueFilter)
            };

            return result;
        }
    }
}
