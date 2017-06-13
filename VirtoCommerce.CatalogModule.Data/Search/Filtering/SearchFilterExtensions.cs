using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public static class SearchFilterExtensions
    {
        public static IList<ISearchFilterValue> GetValues(this ISearchFilter filter)
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

        public static IList<AggregationLabel> GetValueLabels(this IEnumerable<ISearchFilterValue> values)
        {
            var result = values
                .SelectMany(GetValueLabels)
                .Where(l => !string.IsNullOrEmpty(l.Language) && !string.IsNullOrEmpty(l.Label))
                .GroupBy(v => v.Language, StringComparer.OrdinalIgnoreCase)
                .SelectMany(g => g
                    .GroupBy(g2 => g2.Label, StringComparer.OrdinalIgnoreCase)
                    .Select(g2 => g2.FirstOrDefault()))
                .OrderBy(v => v.Language)
                .ThenBy(v => v.Label)
                .ToArray();

            return result.Any() ? result : null;
        }

        private static IList<AggregationLabel> GetValueLabels(this ISearchFilterValue value)
        {
            var result = new List<AggregationLabel>();

            var attributeFilterValue = value as AttributeFilterValue;
            var rangeFilterValue = value as RangeFilterValue;

            if (attributeFilterValue != null)
            {
                result.Add(new AggregationLabel { Language = attributeFilterValue.Language, Label = attributeFilterValue.Value });
            }
            else if (rangeFilterValue?.Displays != null)
            {
                var labels = rangeFilterValue.Displays
                    .Select(d => new AggregationLabel { Language = d.Language, Label = d.Value })
                    .ToArray();
                result.AddRange(labels);
            }

            return result;
        }

        public static ISearchFilter Convert(this ISearchFilter filter, IList<string> keys)
        {
            ISearchFilter result = null;

            if (filter != null && keys != null)
            {
                // get values that we have filters set for
                var values = from v in filter.GetValues() where keys.Contains(v.Id, StringComparer.OrdinalIgnoreCase) select v;

                var attributeFilter = filter as AttributeFilter;
                var rangeFilter = filter as RangeFilter;
                var priceRangeFilter = filter as PriceRangeFilter;
                var categoryFilter = filter as CategoryFilter;

                if (attributeFilter != null)
                {
                    var newFilter = new AttributeFilter();
                    newFilter.InjectFrom(filter);
                    newFilter.Values = values.OfType<AttributeFilterValue>().ToArray();
                    result = newFilter;
                }
                else if (rangeFilter != null)
                {
                    var newFilter = new RangeFilter();
                    newFilter.InjectFrom(filter);

                    newFilter.Values = values.OfType<RangeFilterValue>().ToArray();
                    result = newFilter;
                }
                else if (priceRangeFilter != null)
                {
                    var newFilter = new PriceRangeFilter();
                    newFilter.InjectFrom(filter);

                    newFilter.Values = values.OfType<RangeFilterValue>().ToArray();
                    result = newFilter;
                }
                else if (categoryFilter != null)
                {
                    var newFilter = new CategoryFilter();
                    newFilter.InjectFrom(filter);
                    newFilter.Values = values.OfType<CategoryFilterValue>().ToArray();
                    result = newFilter;
                }
            }

            return result;
        }

        public static AttributeFilterValue[] CreateAttributeFilterValues(this IEnumerable<string> values)
        {
            return values.Select(v => new AttributeFilterValue { Id = v, Value = v }).ToArray();
        }
    }
}
