using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search;

public static class SearchServiceExtensions
{
    public static async IAsyncEnumerable<PropertySearchResult> SearchBatches(this IPropertySearchService propertySearchService, PropertySearchCriteria searchCriteria)
    {
        int totalCount;

        do
        {
            var searchResult = await propertySearchService.SearchPropertiesAsync(searchCriteria);

            if (searchCriteria.Take == 0 ||
                searchResult.Results.Any())
            {
                yield return searchResult;
            }

            if (searchCriteria.Take == 0)
            {
                yield break;
            }

            totalCount = searchResult.TotalCount;
            searchCriteria.Skip += searchCriteria.Take;
        }
        while (searchCriteria.Skip < totalCount);
    }
}
