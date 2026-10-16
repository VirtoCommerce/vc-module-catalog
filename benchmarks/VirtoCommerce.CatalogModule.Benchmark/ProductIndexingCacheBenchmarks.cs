using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Benchmark;

/// <summary>
/// One indexing batch: <see cref="ProductDocumentBuilder.GetDocumentsAsync"/> for <see cref="BatchSize"/> products
/// while the product cache holds <see cref="CachedProducts"/> products, including the cache expiration the builder
/// does at the end of the batch. Item and search services are mocked; the memory cache and its change tokens are real.
/// </summary>
/// <remarks>
/// The builder ends each batch by removing products from the cache, so every iteration needs a freshly filled cache:
/// <see cref="IterationSetup"/> refills it outside the measured region, which makes BenchmarkDotNet pin one
/// invocation per iteration.
/// Server GC matches the platform's ASP.NET Core host, which matters for any forced collection.
/// </remarks>
[MemoryDiagnoser]
[GcServer(true)]
public class ProductIndexingCacheBenchmarks
{
    private const int CacheEntriesPerProduct = 2;

    private ProductDocumentBuilder _builder;
    private List<CatalogProduct> _products;
    private List<string> _batchIds;
    private MemoryCache _cache;

    [Params(100, 200)]
    public int BatchSize { get; set; }

    [Params(10_000, 100_000)]
    public int CachedProducts { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _products = Enumerable.Range(0, CachedProducts).Select(_ => CreateProduct(Guid.NewGuid().ToString("N"))).ToList();
        _batchIds = _products.Take(BatchSize).Select(x => x.Id).ToList();

        var productsById = _products.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

        var itemService = new Mock<IItemService>();
        itemService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => ids.Select(id => productsById[id]).ToList());

        var productSearchService = new Mock<IProductSearchService>();
        productSearchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new ProductSearchResult { Results = [], TotalCount = 0 });

        _builder = new BenchmarkProductDocumentBuilder(itemService.Object, productSearchService.Object);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Start every iteration from the same state: a cache that holds all products, each tied to its key token
        // the way ItemService.ConfigureCache ties it
        _cache?.Dispose();
        ItemCacheRegion.ExpireRegion();
        _cache = new MemoryCache(new MemoryCacheOptions());

        foreach (var product in _products)
        {
            for (var i = 0; i < CacheEntriesPerProduct; i++)
            {
                using var entry = _cache.CreateEntry($"itemservice:getasync-{i}-{product.Id}");
                entry.Value = product;
                entry.AddExpirationToken(ItemCacheRegion.CreateChangeTokenForKey(product.Id));
            }
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _cache?.Dispose();
        ItemCacheRegion.ExpireRegion();
    }

    [Benchmark]
    public Task<IList<IndexDocument>> GetDocuments()
    {
        return _builder.GetDocumentsAsync(_batchIds);
    }

    private static CatalogProduct CreateProduct(string id)
    {
        return new CatalogProduct
        {
            Id = id,
            Code = id,
            Name = "Product " + id,
            IsActive = true,
            Outlines = new List<Outline>(),
            Properties = Enumerable.Range(0, 5).Select(i => CreateProperty(id, i)).ToList(),
            Reviews = [new EditorialReview { Content = new string('x', 200), LanguageCode = "en-US", ReviewType = "QuickReview" }],
            SeoInfos = new List<SeoInfo>(),
        };
    }

    private static Property CreateProperty(string productId, int index)
    {
        var name = "Property" + index;

        return new Property
        {
            Id = productId + index,
            Name = name,
            Type = PropertyType.Product,
            ValueType = PropertyValueType.ShortText,
            Values = [new PropertyValue { PropertyName = name, ValueType = PropertyValueType.ShortText, Value = "Value " + index }],
        };
    }

    private sealed class BenchmarkProductDocumentBuilder(IItemService itemService, IProductSearchService productSearchService)
        : ProductDocumentBuilder(Mock.Of<ISettingsManager>(), Mock.Of<IPropertySearchService>(), itemService, productSearchService, measureService: null)
    {
        protected override bool StoreObjectsInIndex => false;
        protected override bool IndexLinkPriorityFields => false;
    }
}
