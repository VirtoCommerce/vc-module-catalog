using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearch
    {
        public IList<string> ProductIds { get; set; }

        /// <summary>
        /// ItemResponseGroup
        /// </summary>
        public string ResponseGroup { get; set; }

        public string Currency { get; set; }

        public string[] Terms { get; set; }

        public string SearchPhrase { get; set; }
        public string Locale { get; set; }

        /// <summary>
        /// CategoryId1/CategoryId2, no catalog should be included in the outline
        /// </summary>
        public string Outline { get; set; }

        public string[] PriceLists { get; set; }

        public string[] Sort { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; } = 20;


        public virtual T AsCriteria<T>(string catalog, IList<ISearchFilter> allFilters)
            where T : ProductSearchCriteria, new()
        {
            var criteria = AbstractTypeFactory<T>.TryCreateInstance();

            criteria.Ids = ProductIds;
            criteria.Currency = Currency;
            criteria.Pricelists = PriceLists;
            criteria.SearchPhrase = SearchPhrase;
            criteria.Locale = Locale;
            criteria.Skip = Skip;
            criteria.Take = Take;

            var outline = string.Join("/", catalog, Outline).TrimEnd('/', '*').ToLowerInvariant();
            if (!string.IsNullOrEmpty(outline))
            {
                criteria.Outlines.Add(outline);
            }

            var filters = allFilters
                .Where(f => !(f is PriceRangeFilter) || ((PriceRangeFilter)f).Currency.EqualsInvariant(Currency))
                .ToList();

            criteria.Filters = GetFacets(filters);
            criteria.CurrentFilters = GetFilters(filters, Terms);
            criteria.Sorting = GetSorting(criteria, catalog);

            return criteria;
        }


        private static IList<ISearchFilter> GetFacets(IList<ISearchFilter> filters)
        {
            var result = new List<ISearchFilter>();

            if (filters != null)
            {
                result.AddRange(filters);
            }

            return result;
        }

        private static IList<ISearchFilter> GetFilters(IList<ISearchFilter> filters, IEnumerable<string> termStrings)
        {
            var result = new List<ISearchFilter>();

            var terms = termStrings.AsKeyValues();
            if (terms.Any())
            {
                var filtersWithValues = filters
                    ?.Select(x => new { Filter = x, Values = x.GetValues() })
                    .ToList();

                foreach (var term in terms)
                {
                    var filter = filters?.SingleOrDefault(x => x.Key.EqualsInvariant(term.Key));

                    // handle special filter term with a key = "tags", it contains just values and we need to determine which filter to use
                    if (filter == null && term.Key == "tags")
                    {
                        foreach (var termValue in term.Values)
                        {
                            // try to find filter by value
                            var foundFilter = filtersWithValues?.FirstOrDefault(x => x.Values.Any(y => y.Id.Equals(termValue)));

                            if (foundFilter != null)
                            {
                                filter = foundFilter.Filter;

                                var appliedFilter = filter.Convert(term.Values);
                                result.Add(appliedFilter);
                            }
                        }
                    }
                    else if (filter != null) // predefined filter
                    {
                        var attributeFilter = filter as AttributeFilter;
                        if (attributeFilter != null && attributeFilter.Values == null)
                        {
                            filter = new AttributeFilter
                            {
                                Key = attributeFilter.Key,
                                Values = term.Values.CreateAttributeFilterValues(),
                                IsLocalized = attributeFilter.IsLocalized,
                                DisplayNames = attributeFilter.DisplayNames,
                            };
                        }

                        var appliedFilter = filter.Convert(term.Values);
                        result.Add(appliedFilter);
                    }
                    else // custom term
                    {
                        if (!term.Key.StartsWith("_")) // ignore system terms, we can't filter by them
                        {
                            result.Add(CreateAttributeFilter(term.Key, term.Values));
                        }
                    }
                }
            }

            return result;
        }

        protected virtual IList<SortingField> GetSorting<T>(T criteria, string catalog) where T : ProductSearchCriteria, new()
        {
            var result = new List<SortingField>();

            var categoryId = Outline.AsCategoryId();
            var sorts = Sort.AsSortInfoes();
            var priorityFieldName = string.Format(CultureInfo.InvariantCulture, "priority_{0}_{1}", catalog, categoryId).ToLower();

            if (!sorts.IsNullOrEmpty())
            {
                foreach (var sortInfo in sorts)
                {
                    var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                    var isDescending = sortInfo.SortDirection == SortDirection.Descending;

                    switch (fieldName)
                    {
                        case "price":
                            if (criteria.Pricelists != null)
                            {
                                result.AddRange(
                                    criteria.Pricelists.Select(priceList => new SortingField($"price_{criteria.Currency}_{priceList}".ToLower(), isDescending)));
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
            }

            if (!result.Any())
            {
                result.Add(new SortingField(priorityFieldName, true));
                result.Add(new SortingField("priority", true));
                result.Add(ProductSearchCriteria.DefaultSortOrder);
            }

            return result;
        }

        private static ISearchFilter CreateAttributeFilter(string key, IEnumerable<string> values)
        {
            return new AttributeFilter
            {
                Key = key,
                Values = values.Select(v => new AttributeFilterValue { Value = v }).ToArray(),
            };
        }
    }
}
