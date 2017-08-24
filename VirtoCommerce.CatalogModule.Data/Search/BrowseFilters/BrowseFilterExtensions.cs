using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public static class BrowseFilterExtensions
    {
        public static IList<IBrowseFilterValue> GetValues(this IBrowseFilter filter)
        {
            IBrowseFilterValue[] result = null;

            var attributeFilter = filter as AttributeFilter;
            var rangeFilter = filter as RangeFilter;
            var priceRangeFilter = filter as PriceRangeFilter;

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

            return result;
        }
    }
}
