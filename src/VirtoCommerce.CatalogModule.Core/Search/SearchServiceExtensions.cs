using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Search;

public static class SearchServiceExtensions
{
    public static async Task<IList<ListEntryBase>> SearchAllAsync(this IInternalListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, CancellationToken cancellationToken)
    {
        var result = new List<ListEntryBase>();

        await foreach (var searchResult in searchService.SearchBatchesAsync(searchCriteria, cancellationToken))
        {
            result.AddRange(searchResult.Results);
        }

        return result;
    }

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static Task<IList<ListEntryBase>> SearchAllAsync(this IInternalListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchAllAsync(searchCriteria, CancellationToken.None);

    public static async IAsyncEnumerable<ListEntrySearchResult> SearchBatchesAsync(this IInternalListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, [EnumeratorCancellation] CancellationToken cancellationToken)
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

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static IAsyncEnumerable<ListEntrySearchResult> SearchBatchesAsync(this IInternalListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchBatchesAsync(searchCriteria, CancellationToken.None);

    public static async Task<IList<ListEntryBase>> SearchAllAsync(this IListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, CancellationToken cancellationToken)
    {
        var result = new List<ListEntryBase>();

        await foreach (var searchResult in searchService.SearchBatchesAsync(searchCriteria, cancellationToken))
        {
            result.AddRange(searchResult.Results);
        }

        return result;
    }

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static Task<IList<ListEntryBase>> SearchAllAsync(this IListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchAllAsync(searchCriteria, CancellationToken.None);

    public static async IAsyncEnumerable<ListEntrySearchResult> SearchBatchesAsync(this IListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, [EnumeratorCancellation] CancellationToken cancellationToken)
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

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static IAsyncEnumerable<ListEntrySearchResult> SearchBatchesAsync(this IListEntrySearchService searchService, CatalogListEntrySearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchBatchesAsync(searchCriteria, CancellationToken.None);


    public static async Task<IList<CategoryLink>> SearchAllAsync(this ILinkSearchService searchService, LinkSearchCriteria searchCriteria, CancellationToken cancellationToken)
    {
        var result = new List<CategoryLink>();

        await foreach (var searchResult in searchService.SearchBatchesAsync(searchCriteria, cancellationToken))
        {
            result.AddRange(searchResult.Results);
        }

        return result;
    }

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static Task<IList<CategoryLink>> SearchAllAsync(this ILinkSearchService searchService, LinkSearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchAllAsync(searchCriteria, CancellationToken.None);

    public static async IAsyncEnumerable<LinkSearchResult> SearchBatchesAsync(this ILinkSearchService searchService, LinkSearchCriteria searchCriteria, [EnumeratorCancellation] CancellationToken cancellationToken)
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

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static IAsyncEnumerable<LinkSearchResult> SearchBatchesAsync(this ILinkSearchService searchService, LinkSearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchBatchesAsync(searchCriteria, CancellationToken.None);

    public static async IAsyncEnumerable<ProductIndexedSearchResult> SearchBatchesAsync(this IProductIndexedSearchService searchService, ProductIndexedSearchCriteria searchCriteria, [EnumeratorCancellation] CancellationToken cancellationToken)
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

    [Obsolete("Use the cancellation-aware overload instead.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static IAsyncEnumerable<ProductIndexedSearchResult> SearchBatchesAsync(this IProductIndexedSearchService searchService, ProductIndexedSearchCriteria searchCriteria, ICancellationToken cancellationToken)
        => searchService.SearchBatchesAsync(searchCriteria, CancellationToken.None);

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
