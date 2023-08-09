using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Search.Indexing
{
    public class ProductSearchRequestBuilder : ISearchRequestBuilder
    {
        private readonly ISearchPhraseParser _searchPhraseParser;
        private readonly ITermFilterBuilder _termFilterBuilder;
        private readonly IAggregationConverter _aggregationConverter;

        public ProductSearchRequestBuilder(ISearchPhraseParser searchPhraseParser, ITermFilterBuilder termFilterBuilder, IAggregationConverter aggregationConverter)
        {
            _searchPhraseParser = searchPhraseParser;
            _termFilterBuilder = termFilterBuilder;
            _aggregationConverter = aggregationConverter;
        }

        public virtual string DocumentType { get; } = KnownDocumentTypes.Product;

        public virtual async Task<SearchRequest> BuildRequestAsync(SearchCriteriaBase criteria)
        {
            SearchRequest request = null;

            if (criteria is ProductIndexedSearchCriteria productSearchCriteria)
            {
                // GetAllFiltersAsync() modifies Keyword
                productSearchCriteria = productSearchCriteria.CloneTyped();
                var allFilters = await GetAllFiltersAsync(productSearchCriteria);

                request = new SearchRequest
                {
                    SearchKeywords = productSearchCriteria.Keyword,
                    SearchFields = new[] { "__content" },
                    Filter = GetFilters(allFilters).And(),
                    Sorting = GetSorting(productSearchCriteria),
                    Skip = criteria.Skip,
                    Take = criteria.Take,
                    Aggregations = _aggregationConverter == null ? null : await _aggregationConverter.GetAggregationRequestsAsync(productSearchCriteria, allFilters),
                    IsFuzzySearch = productSearchCriteria.IsFuzzySearch,
                    //RawQuery = productSearchCriteria.RawQuery
                };
            }

            return request;
        }


        protected virtual IList<SortingField> GetSorting(ProductIndexedSearchCriteria criteria)
        {
            var result = new List<SortingField>();
            //For sorting by relevance have to keep sortInfo clear
            var needClearSortInfo = false;

            var priorityFields = criteria.GetPriorityFields();

            foreach (var sortInfo in criteria.SortInfos)
            {
                var sortingField = new SortingField();
                if (sortInfo is GeoSortInfo geoSortInfo)
                {
                    sortingField = new GeoDistanceSortingField
                    {
                        Location = geoSortInfo.GeoPoint
                    };
                }
                sortingField.FieldName = sortInfo.SortColumn.ToLowerInvariant();
                sortingField.IsDescending = sortInfo.SortDirection == SortDirection.Descending;

                switch (sortingField.FieldName)
                {
                    case "relevance":
                        needClearSortInfo = true;
                        break;
                    case "price":
                        if (!criteria.Pricelists.IsNullOrEmpty())
                        {
                            result.AddRange(
                                criteria.Pricelists.Select(priceList => new SortingField($"price_{criteria.Currency}_{priceList}".ToLowerInvariant(), sortingField.IsDescending)));
                        }
                        else
                        {
                            result.Add(new SortingField($"price_{criteria.Currency}".ToLowerInvariant(), sortingField.IsDescending));
                        }
                        break;
                    case "priority":
                        result.AddRange(priorityFields.Select(priorityField => new SortingField(priorityField, sortingField.IsDescending)));
                        break;
                    case "name":
                    case "title":
                        result.Add(new SortingField("name", sortingField.IsDescending));
                        break;
                    default:
                        result.Add(sortingField);
                        break;
                }
            }

            if (!result.Any())
            {
                result.AddRange(priorityFields.Select(priorityField => new SortingField(priorityField, true)));
                result.Add(new SortingField("__sort"));
            }

            if (needClearSortInfo)
            {
                result.Clear();
            }

            return result;
        }

        protected virtual IList<IFilter> GetFilters(FiltersContainer allFilters)
        {
            return allFilters.GetFiltersExceptSpecified(null);
        }

        protected virtual async Task<FiltersContainer> GetAllFiltersAsync(ProductIndexedSearchCriteria criteria)
        {
            var permanentFilters = GetPermanentFilters(criteria);
            var termFilters = await _termFilterBuilder.GetTermFiltersAsync(criteria);

            var result = new FiltersContainer
            {
                PermanentFilters = permanentFilters.Concat(termFilters.PermanentFilters).ToList(),
                RemovableFilters = termFilters.RemovableFilters,
            };

            return result;
        }

        protected virtual IList<IFilter> GetPermanentFilters(ProductIndexedSearchCriteria criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.Keyword);
                criteria.Keyword = parseResult.Keyword;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.ObjectIds != null)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            result.AddCatalogFilters(criteria);

            result.Add(FiltersHelper.CreateOutlineFilter(criteria));

            if (criteria.StartDateFrom.HasValue)
            {
                result.Add(FiltersHelper.CreateDateRangeFilter("startdate", criteria.StartDateFrom, criteria.StartDate, false, true));
            }

            if (criteria.EndDate.HasValue)
            {
                result.Add(FiltersHelper.CreateDateRangeFilter("enddate", criteria.EndDate, null, false, false));
            }

            if (!criteria.ClassTypes.IsNullOrEmpty())
            {
                result.Add(FiltersHelper.CreateTermFilter("__type", criteria.ClassTypes));
            }

            if (criteria.SearchInVariations)
            {
                result.Add(FiltersHelper.CreateTermFilter("is", new[] { "variation", "product" }));
            }
            else if (!criteria.WithHidden)
            {
                result.Add(FiltersHelper.CreateTermFilter("status", "visible"));
            }

            if (criteria.PriceRange != null)
            {
                var range = criteria.PriceRange;
                result.Add(FiltersHelper.CreatePriceRangeFilter(criteria.Currency, criteria.Pricelists, range.Lower, range.Upper, range.IncludeLower, range.IncludeUpper));
            }

            if (criteria.GeoDistanceFilter != null)
            {
                result.Add(criteria.GeoDistanceFilter);
            }

            return result;
        }
    }
}
