using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Seo.Core.Models;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

// Tests in this class change the static CancellableCacheRegion.OnTokenCancelled hook, so they must not run in parallel with each other
[Collection(nameof(ProductDocumentBuilderCacheTests))]
public class ProductDocumentBuilderCacheTests
{
    [Fact]
    public async Task GetDocumentsAsync_DoesNotExpireCacheOfOtherProducts()
    {
        // Arrange
        var productId = NewId();
        var variationId = NewId();
        var otherProductId = NewId();
        var builder = GetProductDocumentBuilder(productId, variationId);

        var otherProductToken = ItemCacheRegion.CreateChangeTokenForKey(otherProductId);

        // Act
        await builder.GetDocumentsAsync([productId]);

        // Assert
        otherProductToken.HasChanged.Should().BeFalse("indexing a product must not evict other products from the cache");
    }

    [Fact]
    public async Task GetDocumentsAsync_ExpiresIndexedProductsLocally()
    {
        // Arrange
        var productId = NewId();
        var variationId = NewId();
        var builder = GetProductDocumentBuilder(productId, variationId);

        var productToken = ItemCacheRegion.CreateChangeTokenForKey(productId);
        var variationToken = ItemCacheRegion.CreateChangeTokenForKey(variationId);

        // Act
        await builder.GetDocumentsAsync([productId]);

        // Assert
        productToken.HasChanged.Should().BeTrue("products loaded for indexing should not stay in the cache");
        variationToken.HasChanged.Should().BeTrue("variations loaded for indexing should not stay in the cache");
    }

    [Fact]
    public async Task GetDocumentsAsync_DoesNotPropagateCacheExpirationToOtherInstances()
    {
        // Arrange
        var productId = NewId();
        var variationId = NewId();
        var builder = GetProductDocumentBuilder(productId, variationId);

        var regionTokenKey = ItemCacheRegion.GenerateRegionTokenKey();
        var testTokenKeys = new[] { regionTokenKey, ItemCacheRegion.GenerateRegionTokenKey(productId), ItemCacheRegion.GenerateRegionTokenKey(variationId) };
        var propagatedTokenKeys = new List<string>();

        var originalHandler = CancellableCacheRegion.OnTokenCancelled;
        CancellableCacheRegion.OnTokenCancelled = args =>
        {
            if (testTokenKeys.Contains(args.TokenKey, StringComparer.OrdinalIgnoreCase))
            {
                lock (propagatedTokenKeys)
                {
                    propagatedTokenKeys.Add(args.TokenKey);
                }
            }
        };

        try
        {
            // Act
            await builder.GetDocumentsAsync([productId]);
        }
        finally
        {
            CancellableCacheRegion.OnTokenCancelled = originalHandler;
        }

        // Assert
        propagatedTokenKeys.Should().BeEmpty("the indexed data is not stale, so other instances must keep their cache");
    }


    private static string NewId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static ProductDocumentBuilder GetProductDocumentBuilder(string productId, string variationId)
    {
        var product = CreateProduct(productId, mainProductId: null);
        var variation = CreateProduct(variationId, mainProductId: productId);

        var itemService = new Mock<IItemService>();
        itemService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => ids.Contains(productId) ? [product] : []);

        var productSearchService = new Mock<IProductSearchService>();
        productSearchService
            .Setup(x => x.SearchAsync(It.IsAny<ProductSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((ProductSearchCriteria criteria, bool _) => new ProductSearchResult
            {
                Results = criteria.MainProductId == productId && criteria.Skip == 0 ? [variation] : [],
                TotalCount = criteria.MainProductId == productId ? 1 : 0,
            });

        return new TestableProductDocumentBuilder(
            Mock.Of<ISettingsManager>(),
            Mock.Of<IPropertySearchService>(),
            itemService.Object,
            productSearchService.Object);
    }

    private static CatalogProduct CreateProduct(string id, string mainProductId)
    {
        return new CatalogProduct
        {
            Id = id,
            Code = id,
            Name = id,
            MainProductId = mainProductId,
            IsActive = true,
            Outlines = new List<Outline>(),
            Properties = new List<Property>(),
            Reviews = new List<EditorialReview>(),
            SeoInfos = new List<SeoInfo>(),
        };
    }

    private sealed class TestableProductDocumentBuilder(
        ISettingsManager settingsManager,
        IPropertySearchService propertySearchService,
        IItemService itemService,
        IProductSearchService productsSearchService)
        : ProductDocumentBuilder(settingsManager, propertySearchService, itemService, productsSearchService, measureService: null)
    {
        protected override bool StoreObjectsInIndex => false;
        protected override bool IndexLinkPriorityFields => false;
    }
}
