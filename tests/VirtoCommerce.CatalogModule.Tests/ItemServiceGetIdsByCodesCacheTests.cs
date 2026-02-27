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
    private const string CatalogId = "catalog-1";
    private const string ProductId = "product-1";
    private const string ProductCode = "SKU-1";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICatalogRepository> _repositoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<AbstractValidator<IHasProperties>> _hasPropertyValidatorMock = new();
    private readonly Mock<ICatalogService> _catalogServiceMock = new();
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly Mock<IOutlineService> _outlineServiceMock = new();
    private readonly Mock<IBlobUrlResolver> _blobUrlResolverMock = new();
    private readonly Mock<ISkuGenerator> _skuGeneratorMock = new();
    private readonly Mock<IPropertyValueSanitizer> _propertyValueSanitizerMock = new();

    public ItemServiceGetIdsByCodesCacheTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _catalogServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
            .ReturnsAsync([new Catalog { Id = CatalogId }]);

        _categoryServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
            .ReturnsAsync([]);

        _hasPropertyValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task GetIdsByCodes_CalledTwiceWithSameCode_LoadsFromRepositoryOnce()
    {
        // Arrange
        var service = CreateService();
        SetupItemsQuery([CreateItemEntity(ProductCode)]);

        // Act
        var firstResult = await service.GetIdsByCodes(CatalogId, [ProductCode]);
        var secondResult = await service.GetIdsByCodes(CatalogId, [ProductCode]);

        // Assert
        firstResult[ProductCode].Should().Be(ProductId);
        secondResult.Should().BeEquivalentTo(firstResult);
        _repositoryMock.Verify(x => x.Items, Times.Once());
    }

    [Fact]
    public async Task GetIdsByCodes_AfterSave_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = CreateService();
        SetupItemsQuery([CreateItemEntity(ProductCode)]);
        SetupForSave();

        await service.GetIdsByCodes(CatalogId, [ProductCode]);
        SetupItemsQuery([]);

        // Act
        await service.SaveChangesAsync([new CatalogProduct { Id = ProductId, CatalogId = CatalogId, Code = "SKU-NEW", Name = "Test" }]);
        var result = await service.GetIdsByCodes(CatalogId, [ProductCode]);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.Items, Times.Exactly(2));
    }

    [Fact]
    public async Task GetIdsByCodes_AfterDelete_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = CreateService();
        SetupForLoad([CreateItemEntity(ProductCode)]);

        await service.GetIdsByCodes(CatalogId, [ProductCode]);
        SetupItemsQuery([]);

        // Act
        await service.DeleteAsync([ProductId]);
        var result = await service.GetIdsByCodes(CatalogId, [ProductCode]);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.Items, Times.Exactly(2));
    }

    private TestableItemService CreateService()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var platformMemoryCache = new PlatformMemoryCache(
            memoryCache,
            Options.Create(new CachingOptions()),
            new Mock<ILogger<PlatformMemoryCache>>().Object);

        return new TestableItemService(
            () => _repositoryMock.Object,
            platformMemoryCache,
            _eventPublisherMock.Object,
            _hasPropertyValidatorMock.Object,
            _catalogServiceMock.Object,
            _categoryServiceMock.Object,
            _outlineServiceMock.Object,
            _blobUrlResolverMock.Object,
            _skuGeneratorMock.Object,
            new ProductValidator(new PropertyValidator()),
            _propertyValueSanitizerMock.Object);
    }

    private void SetupItemsQuery(IList<ItemEntity> items)
    {
        _repositoryMock.Setup(x => x.Items).Returns(items.BuildMockDbSet().Object);
    }

    private void SetupForLoad(IList<ItemEntity> items)
    {
        SetupItemsQuery(items);

        _repositoryMock
            .Setup(x => x.GetItemByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IList<string> ids, string _) =>
                items.Where(x => ids.Contains(x.Id)).ToList());
    }

    private void SetupForSave()
    {
        _repositoryMock
            .Setup(x => x.GetItemByIdsAsync(It.IsAny<IList<string>>(), null))
            .ReturnsAsync([CreateItemEntity(ProductCode)]);
    }

    private static ItemEntity CreateItemEntity(string code) =>
        new()
        {
            Id = ProductId,
            Code = code,
            Name = "Test Product",
            CatalogId = CatalogId,
            IsActive = true,
        };

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
        protected override Task BeforeSaveChanges(IList<CatalogProduct> models) => Task.CompletedTask;
    }
}
