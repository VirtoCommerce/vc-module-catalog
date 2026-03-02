using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

public class ItemServiceGetIdsByCodesCacheTests
{
    [Fact]
    public async Task GetIdsByCodes_CalledTwiceWithSameCode_LoadsFromRepositoryOnce()
    {
        // Arrange
        var service = BuildItemServiceWithProduct("product-id", "SKU-1", "catalog-id");

        // Act
        var result1 = await service.GetIdsByCodes("catalog-id", ["SKU-1"]);
        var result2 = await service.GetIdsByCodes("catalog-id", ["SKU-1"]);

        // Assert
        result1["SKU-1"].Should().Be("product-id");
        result2.Should().BeEquivalentTo(result1);
        service.GetIdsByCodesNoCacheCallsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetIdsByCodes_AfterCodeChange_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = BuildItemServiceWithProduct("product-id", "SKU-1", "catalog-id");

        // Act
        var result1 = await service.GetIdsByCodes("catalog-id", ["SKU-1"]);
        await service.SaveChangesAsync([CreateProduct("product-id", "SKU-NEW", "catalog-id")]);
        var result2 = await service.GetIdsByCodes("catalog-id", ["SKU-1"]);

        // Assert
        result1["SKU-1"].Should().Be("product-id");
        result2.Should().BeEmpty();
        service.GetIdsByCodesNoCacheCallsCount.Should().Be(2);
    }

    [Fact]
    public async Task GetIdsByCodes_AfterDelete_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = BuildItemServiceWithProduct("product-id", "SKU-1", "catalog-id");

        // Act
        var result1 = await service.GetIdsByCodes("catalog-id", ["SKU-1"]);
        await service.DeleteAsync(["product-id"]);
        var result2 = await service.GetIdsByCodes("catalog-id", ["SKU-1"]);

        // Assert
        result1["SKU-1"].Should().Be("product-id");
        result2.Should().BeEmpty();
        service.GetIdsByCodesNoCacheCallsCount.Should().Be(2);
    }


    private static TestableItemService BuildItemServiceWithProduct(string productId, string productCode, string catalogId)
    {
        var itemEntity = CreateItemEntity(productId, productCode, catalogId);
        var repository = GetCatalogRepository([itemEntity]);

        return new TestableItemService(
            () => repository,
            GetPlatformMemoryCache(),
            Mock.Of<IEventPublisher>(),
            GetPropertiesValidator(),
            GetCatalogService(catalogId),
            GetCategoryService(),
            Mock.Of<IOutlineService>(),
            Mock.Of<IBlobUrlResolver>(),
            Mock.Of<ISkuGenerator>(),
            new ProductValidator(new PropertyValidator()),
            Mock.Of<IPropertyValueSanitizer>());
    }

    private static ItemEntity CreateItemEntity(string id, string code, string catalogId)
    {
        return new ItemEntity
        {
            Id = id,
            Code = code,
            CatalogId = catalogId,
            Name = Guid.NewGuid().ToString(),
        };
    }

    private static CatalogProduct CreateProduct(string id, string code, string catalogId)
    {
        return new CatalogProduct
        {
            Id = id,
            Code = code,
            CatalogId = catalogId,
            Name = Guid.NewGuid().ToString(),
        };
    }

    private static ICatalogRepository GetCatalogRepository(List<ItemEntity> items)
    {
        var repositoryMock = new Mock<ICatalogRepository>();

        repositoryMock
            .Setup(x => x.Items)
            .Returns(items.BuildMockDbSet().Object);

        repositoryMock
            .Setup(x => x.GetItemByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IList<string> ids, string _) =>
            {
                return items.Where(x => ids.Contains(x.Id)).ToList();
            });

        repositoryMock
            .Setup(x => x.RemoveItemsAsync(It.IsAny<IList<string>>()))
            .Callback((IList<string> ids) =>
            {
                items.RemoveAll(x => ids.Contains(x.Id));
            });

        repositoryMock.Setup(x => x.UnitOfWork).Returns(Mock.Of<IUnitOfWork>());

        return repositoryMock.Object;
    }

    private static PlatformMemoryCache GetPlatformMemoryCache()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var platformMemoryCache = new PlatformMemoryCache(
            memoryCache,
            Options.Create(new CachingOptions()),
            new Mock<ILogger<PlatformMemoryCache>>().Object);

        return platformMemoryCache;
    }

    private static AbstractValidator<IHasProperties> GetPropertiesValidator()
    {
        var hasPropertiesValidatorMock = new Mock<AbstractValidator<IHasProperties>>();

        hasPropertiesValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        return hasPropertiesValidatorMock.Object;
    }

    private static ICatalogService GetCatalogService(string catalogId)
    {
        var catalogServiceMock = new Mock<ICatalogService>();

        catalogServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([new Catalog { Id = catalogId }]);

        return catalogServiceMock.Object;
    }

    private static ICategoryService GetCategoryService()
    {
        var categoryServiceMock = new Mock<ICategoryService>();

        categoryServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([]);

        return categoryServiceMock.Object;
    }

    private sealed class TestableItemService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        AbstractValidator<IHasProperties> hasPropertyValidator,
        ICatalogService catalogService,
        ICategoryService categoryService,
        IOutlineService outlineService,
        IBlobUrlResolver blobUrlResolver,
        ISkuGenerator skuGenerator,
        AbstractValidator<CatalogProduct> productValidator,
        IPropertyValueSanitizer propertyValueSanitizer)
        : ItemService(repositoryFactory, platformMemoryCache, eventPublisher, hasPropertyValidator, catalogService, categoryService, outlineService, blobUrlResolver, skuGenerator, productValidator, propertyValueSanitizer)
    {
        public int GetIdsByCodesNoCacheCallsCount { get; private set; }

        protected override Task<IList<ProductCodeCacheItem>> GetIdsByCodesNoCache(string catalogId, IList<string> codes)
        {
            GetIdsByCodesNoCacheCallsCount++;
            return base.GetIdsByCodesNoCache(catalogId, codes);
        }
    }
}
