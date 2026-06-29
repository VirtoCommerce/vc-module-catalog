using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;

namespace VirtoCommerce.CatalogModule.Data.Search.Sorting;

public class ProductSortingResolverRegistry : IProductSortingResolverRegistry
{
    private readonly IReadOnlyCollection<IProductSortingResolver> _resolvers;
    private readonly Dictionary<string, IProductSortingResolver> _resolversByCode;

    public ProductSortingResolverRegistry(
        IEnumerable<IProductSortingResolver> resolvers,
        ILogger<ProductSortingResolverRegistry> logger)
    {
        var byCode = new Dictionary<string, IProductSortingResolver>(StringComparer.OrdinalIgnoreCase);

        foreach (var resolver in resolvers.Where(x => !string.IsNullOrWhiteSpace(x?.Code)))
        {
            var added = byCode.TryAdd(resolver.Code, resolver);
            if (!added)
            {
                // Collision must never crash sorting/search: keep the first registration, log the shadowed one.
                logger.LogWarning(
                    "Duplicate product search order resolver code '{Code}': '{Ignored}' is ignored because '{Winner}' is already registered.",
                    resolver.Code, resolver.GetType().Name, byCode[resolver.Code].GetType().Name);
            }
        }

        _resolversByCode = byCode;
        _resolvers = byCode.Values
            .OrderBy(x => x.Info?.Order ?? int.MaxValue)
            .ToList();
    }

    public IReadOnlyCollection<IProductSortingResolver> GetAllResolvers()
    {
        return _resolvers;
    }

    public IProductSortingResolver GetResolver(string code)
    {
        return code != null && _resolversByCode.TryGetValue(code, out var resolver) ? resolver : null;
    }
}
