using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public static class SearchFilterExtensions
    {
        public static ISearchFilterValue[] GetValues(this ISearchFilter filter)
        {
            ISearchFilterValue[] result = null;

            var attributeFilter = filter as AttributeFilter;
            var rangeFilter = filter as RangeFilter;
            var priceRangeFilter = filter as PriceRangeFilter;
            var categoryFilter = filter as CategoryFilter;

            if (attributeFilter != null)
            {
                result = attributeFilter.Values?.OfType<ISearchFilterValue>().ToArray();
            }
            else if (rangeFilter != null)
            {
                result = rangeFilter.Values?.OfType<ISearchFilterValue>().ToArray();
            }
            else if (priceRangeFilter != null)
            {
                result = priceRangeFilter.Values?.OfType<ISearchFilterValue>().ToArray();
            }
            else if (categoryFilter != null)
            {
                result = categoryFilter.Values?.OfType<ISearchFilterValue>().ToArray();
            }

            return result;
        }

        public static ISearchFilter Convert(this ISearchFilter filter, string[] keys)
        {
            if (filter != null && keys != null)
            {
                // get values that we have filters set for
                var values = from v in filter.GetValues() where keys.Contains(v.Id, StringComparer.OrdinalIgnoreCase) select v;

                var attributeFilter = filter as AttributeFilter;
                if (attributeFilter != null)
                {
                    var newFilter = new AttributeFilter();
                    newFilter.InjectFrom(filter);
                    newFilter.Values = values.OfType<AttributeFilterValue>().ToArray();
                    return newFilter;
                }

                var rangeFilter = filter as RangeFilter;
                if (rangeFilter != null)
                {
                    var newFilter = new RangeFilter();
                    newFilter.InjectFrom(filter);

                    newFilter.Values = values.OfType<RangeFilterValue>().ToArray();
                    return newFilter;
                }

                var priceRangeFilter = filter as PriceRangeFilter;
                if (priceRangeFilter != null)
                {
                    var newFilter = new PriceRangeFilter();
                    newFilter.InjectFrom(filter);

                    newFilter.Values = values.OfType<RangeFilterValue>().ToArray();
                    return newFilter;
                }

                var categoryFilter = filter as CategoryFilter;
                if (categoryFilter != null)
                {
                    var newFilter = new CategoryFilter();
                    newFilter.InjectFrom(filter);
                    newFilter.Values = values.OfType<CategoryFilterValue>().ToArray();
                    return newFilter;
                }
            }

            return null;
        }

        public static AttributeFilterValue[] CreateAttributeFilterValues(this IEnumerable<string> values)
        {
            return values.Select(v => new AttributeFilterValue { Id = v, Value = v }).ToArray();
        }
    }
}
