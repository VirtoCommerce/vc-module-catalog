using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Aggregates every <see cref="IProductSortingResolver"/> registered in DI (built-in and contributed by other
/// modules). Responsible for deduplicating by <see cref="IProductSortingResolver.Code"/> with a deterministic
/// winner so a code collision never crashes sorting/search.
/// </summary>
public interface IProductSortingResolverRegistry
{
    /// <summary>
    /// All registered resolvers, deduplicated by code and ordered by their default <see cref="ProductSortingInfo.Order"/>.
    /// </summary>
    IReadOnlyCollection<IProductSortingResolver> GetAllResolvers();

    /// <summary>
    /// The resolver for a code (case-insensitive), or <c>null</c> if none is registered.
    /// </summary>
    IProductSortingResolver GetResolver(string code);
}
