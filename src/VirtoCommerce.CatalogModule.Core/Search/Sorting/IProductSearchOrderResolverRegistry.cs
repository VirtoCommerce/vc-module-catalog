using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Search.Sorting;

/// <summary>
/// Aggregates every <see cref="IProductSearchOrderResolver"/> registered in DI (built-in and contributed by other
/// modules). Responsible for deduplicating by <see cref="IProductSearchOrderResolver.Code"/> with a deterministic
/// winner so a code collision never crashes sorting/search.
/// </summary>
public interface IProductSearchOrderResolverRegistry
{
    /// <summary>
    /// All registered resolvers, deduplicated by code and ordered by their default <see cref="ProductSearchOrderInfo.Order"/>.
    /// </summary>
    IReadOnlyCollection<IProductSearchOrderResolver> GetAllResolvers();

    /// <summary>
    /// The resolver for a code (case-insensitive), or <c>null</c> if none is registered.
    /// </summary>
    IProductSearchOrderResolver GetResolver(string code);
}
