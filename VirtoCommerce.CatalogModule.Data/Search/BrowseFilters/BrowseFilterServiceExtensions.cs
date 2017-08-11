using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public static class BrowseFilterServiceExtensions
    {
        public static IList<IBrowseFilter> GetBrowseFilters(this IBrowseFilterService browseFilterService, ProductSearchCriteria criteria)
        {
            var browseFilters = browseFilterService?.GetAllFilters(criteria.StoreId)?.AsQueryable();

            // Check allowed aggregations
            if (criteria?.IncludeAggregations != null)
            {
                browseFilters = browseFilters?.Where(f => criteria.IncludeAggregations.Contains(f.Key, StringComparer.OrdinalIgnoreCase));
            }

            // Check forbidden aggregations
            if (criteria?.ExcludeAggregations != null)
            {
                browseFilters = browseFilters?.Where(f => !criteria.ExcludeAggregations.Contains(f.Key, StringComparer.OrdinalIgnoreCase));
            }

            var result = browseFilters
                ?.Where(f => !(f is PriceRangeFilter) || ((PriceRangeFilter)f).Currency.EqualsInvariant(criteria.Currency))
                .ToList();

            return result;
        }
    }
}
