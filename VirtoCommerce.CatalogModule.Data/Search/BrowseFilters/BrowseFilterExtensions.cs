using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public static class BrowseFilterExtensions
    {
        [Obsolete]
        public static IList<AggregationLabel> GetValueLabels(this IEnumerable<IBrowseFilterValue> values)
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

        [Obsolete]
        public static IList<AggregationLabel> GetValueLabels(this IBrowseFilterValue value)
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

        public static IList<IBrowseFilterValue> GetValues(this IBrowseFilter filter)
        {
            IBrowseFilterValue[] result = null;

            var attributeFilter = filter as AttributeFilter;
            var rangeFilter = filter as RangeFilter;
            var priceRangeFilter = filter as PriceRangeFilter;
            //var categoryFilter = filter as CategoryFilter;

            if (attributeFilter != null)
            {
                result = attributeFilter.Values?.OfType<IBrowseFilterValue>().ToArray();
            }
            else if (rangeFilter != null)
            {
                result = rangeFilter.Values?.OfType<IBrowseFilterValue>().ToArray();
            }
            else if (priceRangeFilter != null)
            {
                result = priceRangeFilter.Values?.OfType<IBrowseFilterValue>().ToArray();
            }
            //else if (categoryFilter != null)
            //{
            //    result = categoryFilter.Values?.OfType<IBrowseFilterValue>().ToArray();
            //}

            return result;
        }

        [Obsolete]
        public static IBrowseFilter Copy(this IBrowseFilter filter, IList<string> keys)
        {
            IBrowseFilter result = null;

            if (filter != null && keys != null)
            {
                // get values that we have filters set for
                var values = filter.GetValues().Where(v => keys.Contains(v.Id, StringComparer.OrdinalIgnoreCase));

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

        [Obsolete]
        public static AttributeFilterValue[] CreateAttributeFilterValues(this IEnumerable<string> values)
        {
            return values.Select(v => new AttributeFilterValue { Id = v, Value = v }).ToArray();
        }
    }
}
