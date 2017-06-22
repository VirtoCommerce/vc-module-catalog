using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using RangeFilter = VirtoCommerce.Domain.Search.RangeFilter;
using RangeFilterValue = VirtoCommerce.Domain.Search.RangeFilterValue;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchRequestBuilder : ISearchRequestBuilder
    {
        private readonly ISearchPhraseParser _searchPhraseParser;
        private readonly IBrowseFilterService _browseFilterService;

        public ProductSearchRequestBuilder(ISearchPhraseParser searchPhraseParser, IBrowseFilterService browseFilterService)
        {
            _searchPhraseParser = searchPhraseParser;
            _browseFilterService = browseFilterService;
        }

        public virtual string DocumentType { get; } = KnownDocumentTypes.Product;

        public virtual SearchRequest BuildRequest(SearchCriteriaBase criteria)
        {
            SearchRequest request = null;

            var productSearchCriteria = criteria as ProductSearchCriteria;
            if (productSearchCriteria != null)
            {
                // Getting filters modifies search phrase
                var allFilters = GetAllFilters(productSearchCriteria);

                request = new SearchRequest
                {
                    SearchKeywords = productSearchCriteria.SearchPhrase,
                    SearchFields = new[] { "__content" },
                    Filter = GetFilters(allFilters).And(),
                    Sorting = GetSorting(productSearchCriteria),
                    Skip = criteria.Skip,
                    Take = criteria.Take,
                    Aggregations = GetAggregationRequests(productSearchCriteria, allFilters),
                };
            }

            return request;
        }


        protected virtual IList<SortingField> GetSorting(ProductSearchCriteria criteria)
        {
            var result = new List<SortingField>();

            var categoryId = criteria.Outline.AsCategoryId();
            var priorityFieldName = StringsHelper.JoinNonEmptyStrings("_", "priority", criteria.CatalogId, categoryId).ToLowerInvariant();

            foreach (var sortInfo in criteria.SortInfos)
            {
                var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                var isDescending = sortInfo.SortDirection == SortDirection.Descending;

                switch (fieldName)
                {
                    case "price":
                        if (criteria.Pricelists != null)
                        {
                            result.AddRange(
                                criteria.Pricelists.Select(priceList => new SortingField($"price_{criteria.Currency}_{priceList}".ToLowerInvariant(), isDescending)));
                        }
                        break;
                    case "priority":
                        result.Add(new SortingField(priorityFieldName, isDescending));
                        result.Add(new SortingField("priority", isDescending));
                        break;
                    case "name":
                    case "title":
                        result.Add(new SortingField("name", isDescending));
                        break;
                    default:
                        result.Add(new SortingField(fieldName, isDescending));
                        break;
                }
            }

            if (!result.Any())
            {
                result.Add(new SortingField(priorityFieldName, true));
                result.Add(new SortingField("priority", true));
                result.Add(new SortingField("__sort"));
            }

            return result;
        }

        protected virtual IList<IFilter> GetFilters(FiltersContainer allFilters)
        {
            return allFilters.GetFiltersExceptSpecified(null);
        }


        private FiltersContainer GetAllFilters(ProductSearchCriteria criteria)
        {
            var permanentFilters = GetPermanentFilters(criteria);
            var removableFilters = GetRemovableFilters(criteria, permanentFilters);

            return new FiltersContainer
            {
                PermanentFilters = permanentFilters,
                RemovableFilters = removableFilters,
            };
        }

        private IList<IFilter> GetPermanentFilters(ProductSearchCriteria criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.SearchPhrase))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.SearchPhrase);
                criteria.SearchPhrase = parseResult.SearchPhrase;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.ObjectIds != null)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            if (!string.IsNullOrEmpty(criteria.CatalogId))
            {
                result.Add(FiltersHelper.CreateTermFilter("catalog", criteria.CatalogId.ToLowerInvariant()));
            }

            if (!criteria.Outline.IsNullOrEmpty())
            {
                var outline = string.Join("/", criteria.CatalogId, criteria.Outline).TrimEnd('/', '*').ToLowerInvariant();
                result.Add(FiltersHelper.CreateTermFilter("__outline", outline));
            }

            result.Add(FiltersHelper.CreateDateRangeFilter("startdate", criteria.StartDateFrom, criteria.StartDate, false, true));

            if (criteria.EndDate != null)
            {
                result.Add(FiltersHelper.CreateDateRangeFilter("enddate", criteria.EndDate, null, false, false));
            }

            if (!criteria.ClassTypes.IsNullOrEmpty())
            {
                result.Add(FiltersHelper.CreateTermFilter("__type", criteria.ClassTypes));
            }

            if (!criteria.WithHidden)
            {
                result.Add(FiltersHelper.CreateTermFilter("status", "visible"));
            }

            if (criteria.PriceRange != null)
            {
                var range = criteria.PriceRange;
                result.Add(FiltersHelper.CreatePriceRangeFilter(criteria.Currency, null, range.Lower, range.Upper, range.IncludeLower, range.IncludeUpper));
            }

            return result;
        }

        private IList<KeyValuePair<string, IFilter>> GetRemovableFilters(ProductSearchCriteria criteria, IList<IFilter> permanentFilters)
        {
            var result = new List<KeyValuePair<string, IFilter>>();

            var terms = criteria.Terms.AsKeyValues();
            if (terms.Any())
            {
                var browseFilters = GetBrowseFilters(criteria);

                var filtersAndValues = browseFilters
                    ?.Select(x => new { Filter = x, Values = x.GetValues() })
                    .ToList();

                foreach (var term in terms)
                {
                    var browseFilter = browseFilters?.SingleOrDefault(x => x.Key.EqualsInvariant(term.Key));

                    // Handle special filter term with a key = "tags", it contains just values and we need to determine which filter to use
                    if (browseFilter == null && term.Key == "tags")
                    {
                        foreach (var termValue in term.Values)
                        {
                            // Try to find filter by value
                            var filterAndValues = filtersAndValues?.FirstOrDefault(x => x.Values.Any(y => y.Id.Equals(termValue)));
                            if (filterAndValues != null)
                            {
                                var filter = ConvertBrowseFilter(filterAndValues.Filter, term.Values, criteria);
                                permanentFilters.Add(filter);
                            }
                        }
                    }
                    else if (browseFilter != null) // Predefined filter
                    {
                        var filter = ConvertBrowseFilter(browseFilter, term.Values, criteria);
                        result.Add(new KeyValuePair<string, IFilter>(browseFilter.Key, filter));
                    }
                    else // Custom term
                    {
                        var filter = FiltersHelper.CreateTermFilter(term.Key, term.Values);
                        permanentFilters.Add(filter);
                    }
                }
            }

            return result;
        }

        private static IFilter ConvertBrowseFilter(IBrowseFilter filter, IList<string> valueIds, ProductSearchCriteria criteria)
        {
            IFilter result = null;

            if (filter != null && valueIds != null)
            {
                var attributeFilter = filter as AttributeFilter;
                var rangeFilter = filter as BrowseFilters.RangeFilter;
                var priceRangeFilter = filter as PriceRangeFilter;

                if (attributeFilter != null)
                {
                    result = ConvertAttributeFilter(attributeFilter, valueIds);
                }
                else if (rangeFilter != null)
                {
                    result = ConvertRangeFilter(rangeFilter, valueIds);
                }
                else if (priceRangeFilter != null)
                {
                    result = ConvertPriceRangeFilter(priceRangeFilter, valueIds, criteria);
                }
            }

            return result;
        }

        private static IFilter ConvertAttributeFilter(AttributeFilter attributeFilter, IList<string> valueIds)
        {
            var result = new TermFilter
            {
                FieldName = attributeFilter.Key,
                Values = attributeFilter.Values
                ?.Where(v => valueIds.Contains(v.Id, StringComparer.OrdinalIgnoreCase))
                .Select(v => v.Value)
                .ToArray() ?? valueIds,
            };

            return result;
        }

        private static IFilter ConvertPriceRangeFilter(PriceRangeFilter priceRangeFilter, IList<string> valueIds, ProductSearchCriteria criteria)
        {
            IFilter result = null;

            if (string.IsNullOrEmpty(criteria.Currency) || priceRangeFilter.Currency.EqualsInvariant(criteria.Currency))
            {
                var filters = priceRangeFilter.Values
                    ?.Where(v => valueIds.Contains(v.Id, StringComparer.OrdinalIgnoreCase))
                    .Select(v => FiltersHelper.CreatePriceRangeFilter(priceRangeFilter.Currency, criteria.Pricelists, v.Lower, v.Upper, v.IncludeLower, v.IncludeUpper))
                    .Where(f => f != null)
                    .ToList();

                result = filters.Or();
            }

            return result;
        }

        private static IFilter ConvertRangeFilter(BrowseFilters.RangeFilter rangeFilter, IList<string> valueIds)
        {
            var result = new RangeFilter
            {
                FieldName = rangeFilter.Key,
                Values = rangeFilter.Values
                    ?.Where(v => valueIds.Contains(v.Id, StringComparer.OrdinalIgnoreCase))
                    .Select(ConvertRangeFilterValue)
                    .ToArray(),
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

        #region Aggregations

        private IList<AggregationRequest> GetAggregationRequests(ProductSearchCriteria criteria, FiltersContainer allFilters)
        {
            var result = new List<AggregationRequest>();

            var browseFilters = GetBrowseFilters(criteria);
            if (browseFilters != null)
            {
                foreach (var filter in browseFilters)
                {
                    var existingFilters = allFilters.GetFiltersExceptSpecified(filter.Key);

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

        private IList<IBrowseFilter> GetBrowseFilters(ProductSearchCriteria criteria)
        {
            var browseFilters = _browseFilterService.GetFilters(criteria.StoreId);

            var result = browseFilters
                ?.Where(f => !(f is PriceRangeFilter) || ((PriceRangeFilter)f).Currency.EqualsInvariant(criteria.Currency))
                .ToList();

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

        #endregion
    }
}
