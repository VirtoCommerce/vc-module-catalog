using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearch : CatalogSearch
    {
        /// <summary>
        /// ItemResponseGroup
        /// </summary>
        public string ResponseGroup { get; set; }

        public string Currency { get; set; }

        public string[] Terms { get; set; }

        public string[] Pricelists { get; set; }

        public NumericRange PriceRange { get; set; }


        public virtual T AsCriteria<T>(string catalog, IList<IBrowseFilter> allFilters)
            where T : ProductSearchCriteria, new()
        {
            var criteria = base.AsCriteria<T>(catalog);

            criteria.Currency = Currency;
            criteria.Pricelists = Pricelists;
            criteria.PriceRange = PriceRange;
            criteria.ResponseGroup = EnumUtility.SafeParse(ResponseGroup, ItemResponseGroup.ItemLarge & ~ItemResponseGroup.ItemProperties);


            var filters = allFilters
                .Where(f => !(f is PriceRangeFilter) || ((PriceRangeFilter)f).Currency.EqualsInvariant(Currency))
                .ToList();

            criteria.BrowseFilters = GetFacets(filters);
            criteria.CurrentFilters = GetFilters(filters, Terms);
            criteria.Sorting = GetSorting(catalog);

            return criteria;
        }


        private static IList<IBrowseFilter> GetFacets(IList<IBrowseFilter> filters)
        {
            var result = new List<IBrowseFilter>();

            if (filters != null)
            {
                result.AddRange(filters);
            }

            return result;
        }

        private static IList<IBrowseFilter> GetFilters(IList<IBrowseFilter> filters, IEnumerable<string> termStrings)
        {
            var result = new List<IBrowseFilter>();

            var terms = termStrings.AsKeyValues();
            if (terms.Any())
            {
                var filtersAndValues = filters
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
                            var filterAndValues = filtersAndValues?.FirstOrDefault(x => x.Values.Any(y => y.Id.Equals(termValue)));
                            if (filterAndValues != null)
                            {
                                filter = filterAndValues.Filter;

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
                        if (!term.Key.StartsWith("_")) // ignore system terms, we can't filter by them // AD: Why???
                        {
                            result.Add(CreateAttributeFilter(term.Key, term.Values));
                        }
                    }
                }
            }

            return result;
        }

        protected virtual IList<SortingField> GetSorting(string catalog)
        {
            var result = new List<SortingField>();

            var categoryId = Outline.AsCategoryId();
            var sorts = Sort.AsSortInfoes();
            var priorityFieldName = StringsHelper.JoinNonEmptyStrings("_", "priority", catalog, categoryId).ToLowerInvariant();

            if (!sorts.IsNullOrEmpty())
            {
                foreach (var sortInfo in sorts)
                {
                    var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                    var isDescending = sortInfo.SortDirection == SortDirection.Descending;

                    switch (fieldName)
                    {
                        case "price":
                            if (Pricelists != null)
                            {
                                result.AddRange(
                                    Pricelists.Select(priceList => new SortingField($"price_{Currency}_{priceList}".ToLowerInvariant(), isDescending)));
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
                result.Add(CatalogSearchCriteria.DefaultSortOrder);
            }

            return result;
        }

        private static IBrowseFilter CreateAttributeFilter(string key, IEnumerable<string> values)
        {
            return new AttributeFilter
            {
                Key = key,
                Values = values.Select(v => new AttributeFilterValue { Value = v }).ToArray(),
            };
        }
    }
}
