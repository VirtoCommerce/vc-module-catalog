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
        public ProductSearch()
        {
            Take = 20;
        }

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

        public int Take { get; set; }

        public virtual T AsCriteria<T>(string storeId, string catalog, IList<ISearchFilter> filters)
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

            // add outline
            if (!string.IsNullOrEmpty(Outline))
            {
                criteria.Outlines.Add(string.Join("/", catalog, Outline));
            }
            else
            {
                criteria.Outlines.Add(catalog ?? string.Empty);
            }

            // Add facets
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    criteria.Add(filter);
                }
            }

            ApplyFilters(criteria, filters);

            criteria.Sorting = GetSorting(catalog, criteria);

            return criteria;
        }

        protected virtual void ApplyFilters<T>(T criteria, IList<ISearchFilter> filters)
            where T : ProductSearchCriteria, new()
        {
            var terms = Terms.AsKeyValues();
            if (terms.Any())
            {
                var filtersWithValues = filters?
                    .Where(x => !(x is PriceRangeFilter) || ((PriceRangeFilter)x).Currency.EqualsInvariant(Currency))
                    .Select(x => new { Filter = x, Values = x.GetValues() })
                    .ToList();

                foreach (var term in terms)
                {
                    var filter = filters?.SingleOrDefault(x => x.Key.EqualsInvariant(term.Key)
                        && (!(x is PriceRangeFilter) || ((PriceRangeFilter)x).Currency.EqualsInvariant(criteria.Currency)));

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
                                criteria.Apply(appliedFilter);
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
                        criteria.Apply(appliedFilter);
                    }
                    else // custom term
                    {
                        if (!term.Key.StartsWith("_")) // ignore system terms, we can't filter by them
                        {
                            var attr = new AttributeFilter { Key = term.Key, Values = term.Values.CreateAttributeFilterValues() };
                            criteria.Apply(attr);
                        }
                    }
                }
            }
        }

        protected virtual List<SortingField> GetSorting<T>(string catalog, T criteria) where T : ProductSearchCriteria, new()
        {
            var categoryId = Outline.AsCategoryId();
            var sorts = Sort.AsSortInfoes();
            var sortFields = new List<SortingField>();
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
                                sortFields.AddRange(
                                    criteria.Pricelists.Select(priceList => new SortingField($"price_{criteria.Currency}_{priceList}".ToLower(), isDescending)));
                            }
                            break;
                        case "priority":
                            sortFields.Add(new SortingField(priorityFieldName, isDescending));
                            sortFields.Add(new SortingField("priority", isDescending));
                            break;
                        case "name":
                        case "title":
                            sortFields.Add(new SortingField("name", isDescending));
                            break;
                        default:
                            sortFields.Add(new SortingField(fieldName, isDescending));
                            break;
                    }
                }
            }

            if (!sortFields.Any())
            {
                sortFields.Add(new SortingField(priorityFieldName, true));
                sortFields.Add(new SortingField("priority", true));
                sortFields.Add(ProductSearchCriteria.DefaultSortOrder);
            }
            return sortFields;
        }
    }
}
