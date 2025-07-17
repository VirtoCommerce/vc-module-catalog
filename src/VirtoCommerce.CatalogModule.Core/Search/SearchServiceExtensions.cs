using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Search;

public static class SearchServiceExtensions
{
    public static async Task<IList<CategoryLink>> SearchAllAsync(this ILinkSearchService searchService, LinkSearchCriteria searchCriteria, ICancellationToken cancellationToken)
    {
        var result = new List<CategoryLink>();

        await foreach (var searchResult in searchService.SearchBatchesAsync(searchCriteria, cancellationToken))
        {
            result.AddRange(searchResult.Results);
        }

        return result;
    }

    public static async IAsyncEnumerable<LinkSearchResult> SearchBatchesAsync(this ILinkSearchService searchService, LinkSearchCriteria searchCriteria, ICancellationToken cancellationToken)
    {
        int totalCount;
        searchCriteria = searchCriteria.CloneTyped();

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var searchResult = await searchService.SearchAsync(searchCriteria);

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

    public static async IAsyncEnumerable<ProductIndexedSearchResult> SearchBatchesAsync(this IProductIndexedSearchService searchService, ProductIndexedSearchCriteria searchCriteria, ICancellationToken cancellationToken)
    {
        long totalCount;
        searchCriteria = searchCriteria.CloneTyped();

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var searchResult = await searchService.SearchAsync(searchCriteria);

            if (searchCriteria.Take == 0 ||
                searchResult.Items.Length > 0)
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

    public static async IAsyncEnumerable<PropertySearchResult> SearchBatches(this IPropertySearchService searchService, PropertySearchCriteria searchCriteria)
    {
        int totalCount;
        searchCriteria = searchCriteria.CloneTyped();

        do
        {
            var searchResult = await searchService.SearchPropertiesAsync(searchCriteria);

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
}
