using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Search;

public static class SearchServiceExtensions
{
    public static async Task<IList<ListEntryBase>> SearchAllAsync(this IInternalListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, ICancellationToken cancellationToken)
    {
        var result = new List<ListEntryBase>();

        await foreach (var searchResult in searchService.SearchBatchesAsync(searchCriteria, cancellationToken))
        {
            result.AddRange(searchResult.Results);
        }

        return result;
    }

    public static async IAsyncEnumerable<ListEntrySearchResult> SearchBatchesAsync(this IInternalListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, ICancellationToken cancellationToken)
    {
        int totalCount;
        searchCriteria = searchCriteria.CloneTyped();

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var searchResult = await searchService.InnerListEntrySearchAsync(searchCriteria);

            if (searchCriteria.Take == 0 ||
                searchResult.Results.Count > 0)
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
