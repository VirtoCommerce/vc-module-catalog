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
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

public class CategoryServiceGetIdsByCodesCacheTests
{
    private const string CatalogId = "catalog-1";
    private const string CategoryId = "category-1";
    private const string CategoryCode = "CODE-1";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICatalogRepository> _repositoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<AbstractValidator<IHasProperties>> _hasPropertyValidatorMock = new();
    private readonly Mock<ICatalogService> _catalogServiceMock = new();
    private readonly Mock<IOutlineService> _outlineServiceMock = new();
    private readonly Mock<IBlobUrlResolver> _blobUrlResolverMock = new();
    private readonly Mock<IPropertyValueSanitizer> _propertyValueSanitizerMock = new();

    public CategoryServiceGetIdsByCodesCacheTests()
    {
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _catalogServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
            .ReturnsAsync([new Catalog { Id = CatalogId }]);

        _hasPropertyValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task GetIdsByCodes_CalledTwiceWithSameCode_LoadsFromRepositoryOnce()
    {
        // Arrange
        var service = CreateService();
        SetupCategoriesQuery([CreateCategoryEntity(CategoryCode)]);

        // Act
        var firstResult = await service.GetIdsByCodes(CatalogId, [CategoryCode]);
        var secondResult = await service.GetIdsByCodes(CatalogId, [CategoryCode]);

        // Assert
        firstResult[CategoryCode].Should().Be(CategoryId);
        secondResult.Should().BeEquivalentTo(firstResult);
        _repositoryMock.Verify(x => x.Categories, Times.Once());
    }

    [Fact]
    public async Task GetIdsByCodes_AfterSave_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = CreateService();
        SetupCategoriesQuery([CreateCategoryEntity(CategoryCode)]);
        SetupForSave();

        await service.GetIdsByCodes(CatalogId, [CategoryCode]);
        SetupCategoriesQuery([]);

        // Act
        await service.SaveChangesAsync([new Category { Id = CategoryId, CatalogId = CatalogId, Code = "CODE-NEW", Name = "Test" }]);
        var result = await service.GetIdsByCodes(CatalogId, [CategoryCode]);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.Categories, Times.Exactly(2));
    }

    [Fact]
    public async Task GetIdsByCodes_AfterDelete_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = CreateService();
        SetupForLoad([CreateCategoryEntity(CategoryCode)]);
        SetupForDelete();

        await service.GetIdsByCodes(CatalogId, [CategoryCode]);
        SetupCategoriesQuery([]);

        // Act
        await service.DeleteAsync([CategoryId]);
        var result = await service.GetIdsByCodes(CatalogId, [CategoryCode]);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.Categories, Times.Exactly(2));
    }

    private TestableCategoryService CreateService()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var platformMemoryCache = new PlatformMemoryCache(
            memoryCache,
            Options.Create(new CachingOptions()),
            new Mock<ILogger<PlatformMemoryCache>>().Object);

        return new TestableCategoryService(
            () => _repositoryMock.Object,
            platformMemoryCache,
            _eventPublisherMock.Object,
            _hasPropertyValidatorMock.Object,
            _catalogServiceMock.Object,
            _outlineServiceMock.Object,
            _blobUrlResolverMock.Object,
            _propertyValueSanitizerMock.Object);
    }

    private void SetupCategoriesQuery(IList<CategoryEntity> categories)
    {
        _repositoryMock.Setup(x => x.Categories).Returns(categories.BuildMockDbSet().Object);
    }

    private void SetupForLoad(IList<CategoryEntity> categories)
    {
        SetupCategoriesQuery(categories);

        _repositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IList<string> ids, string _) =>
                categories.Where(x => ids.Contains(x.Id)).ToList());
    }

    private void SetupForSave()
    {
        _repositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), null))
            .ReturnsAsync([CreateCategoryEntity(CategoryCode)]);

        SetupClearCacheMocks();
    }

    private void SetupForDelete()
    {
        _repositoryMock
            .Setup(x => x.Items)
            .Returns(new List<ItemEntity>().BuildMockDbSet().Object);

        SetupClearCacheMocks();
    }

    private void SetupClearCacheMocks()
    {
        _repositoryMock
            .Setup(x => x.CategoryLinks)
            .Returns(new List<CategoryRelationEntity>().BuildMockDbSet().Object);

        _repositoryMock
            .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync(new List<string>());
    }

    private static CategoryEntity CreateCategoryEntity(string code) =>
        new()
        {
            Id = CategoryId,
            Code = code,
            Name = "Test Category",
            CatalogId = CatalogId,
            IsActive = true,
        };

    private sealed class TestableCategoryService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        AbstractValidator<IHasProperties> hasPropertyValidator,
        ICatalogService catalogService,
        IOutlineService outlineService,
        IBlobUrlResolver blobUrlResolver,
        IPropertyValueSanitizer propertyValueSanitizer)
        : CategoryService(repositoryFactory, platformMemoryCache, eventPublisher, hasPropertyValidator, catalogService, outlineService, blobUrlResolver, propertyValueSanitizer)
    {
        protected override Task BeforeSaveChanges(IList<Category> models) => Task.CompletedTask;
    }
}
