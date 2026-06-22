using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;

namespace VirtoCommerce.CatalogModule.Data.Search.Sorting;

public class ProductSearchOrderResolverRegistry : IProductSearchOrderResolverRegistry
{
    private readonly IReadOnlyCollection<IProductSearchOrderResolver> _resolvers;
    private readonly IDictionary<string, IProductSearchOrderResolver> _resolversByCode;

    public ProductSearchOrderResolverRegistry(
        IEnumerable<IProductSearchOrderResolver> resolvers,
        ILogger<ProductSearchOrderResolverRegistry> logger)
    {
        var byCode = new Dictionary<string, IProductSearchOrderResolver>(StringComparer.OrdinalIgnoreCase);

        foreach (var resolver in resolvers.Where(x => !string.IsNullOrWhiteSpace(x?.Code)))
        {
            if (!byCode.TryAdd(resolver.Code, resolver))
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

    public IReadOnlyCollection<IProductSearchOrderResolver> GetAllResolvers()
    {
        return _resolvers;
    }

    public IProductSearchOrderResolver GetResolver(string code)
    {
        return code != null && _resolversByCode.TryGetValue(code, out var resolver) ? resolver : null;
    }
}
