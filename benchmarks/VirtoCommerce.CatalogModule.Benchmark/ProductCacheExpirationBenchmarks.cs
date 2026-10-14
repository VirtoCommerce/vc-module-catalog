using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Data.Caching;

namespace VirtoCommerce.CatalogModule.Benchmark;

/// <summary>
/// The cache expiration step of an indexing batch on its own: expiring the key token of each of <see cref="BatchSize"/>
/// products, against expiring the whole product region, while the cache holds <see cref="CachedProducts"/> products.
/// </summary>
/// <remarks>
/// Both cases cancel tokens, so every iteration needs fresh ones: <see cref="IterationSetup"/> refills the cache outside
/// the measured region, which makes BenchmarkDotNet pin one invocation per iteration.
/// </remarks>
[MemoryDiagnoser]
[GcServer(true)]
public class ProductCacheExpirationBenchmarks
{
    private const int CacheEntriesPerProduct = 2;

    private List<string> _productIds;
    private List<string> _batchIds;
    private MemoryCache _cache;

    [Params(100, 200)]
    public int BatchSize { get; set; }

    [Params(10_000, 100_000)]
    public int CachedProducts { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _productIds = Enumerable.Range(0, CachedProducts).Select(_ => Guid.NewGuid().ToString("N")).ToList();
        _batchIds = _productIds.Take(BatchSize).ToList();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _cache?.Dispose();
        ItemCacheRegion.ExpireRegion();
        _cache = new MemoryCache(new MemoryCacheOptions());

        foreach (var productId in _productIds)
        {
            for (var i = 0; i < CacheEntriesPerProduct; i++)
            {
                using var entry = _cache.CreateEntry($"itemservice:getasync-{i}-{productId}");
                entry.Value = productId;
                entry.AddExpirationToken(ItemCacheRegion.CreateChangeTokenForKey(productId));
            }
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _cache?.Dispose();
        ItemCacheRegion.ExpireRegion();
    }

    [Benchmark(Baseline = true)]
    public void ExpireRegion()
    {
        ItemCacheRegion.ExpireRegion();
    }

    [Benchmark]
    public void ExpireBatchProducts()
    {
        foreach (var productId in _batchIds)
        {
            ItemCacheRegion.ExpireTokenForKey(productId, propagate: false);
        }
    }
}
