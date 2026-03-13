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
    [Fact]
    public async Task GetIdsByCodes_CalledTwiceWithSameCode_LoadsFromRepositoryOnce()
    {
        // Arrange
        var service = GetCategoryService("category-id", "CODE", "catalog-id");

        // Act
        var result1 = await service.GetIdsByCodes("catalog-id", ["CODE"]);
        var result2 = await service.GetIdsByCodes("catalog-id", ["CODE"]);

        // Assert
        result1["CODE"].Should().Be("category-id");
        result2.Should().BeEquivalentTo(result1);
        service.GetIdsByCodesNoCacheCallsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetIdsByCodes_AfterCodeChange_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = GetCategoryService("category-id", "CODE", "catalog-id");

        // Act
        var result1 = await service.GetIdsByCodes("catalog-id", ["CODE"]);
        await service.SaveChangesAsync([CreateCategory("category-id", "CODE-NEW", "catalog-id")]);
        var result2 = await service.GetIdsByCodes("catalog-id", ["CODE"]);

        // Assert
        result1["CODE"].Should().Be("category-id");
        result2.Should().BeEmpty();
        service.GetIdsByCodesNoCacheCallsCount.Should().Be(2);
    }

    [Fact]
    public async Task GetIdsByCodes_AfterDelete_DoesNotReturnCachedValue()
    {
        // Arrange
        var service = GetCategoryService("category-id", "CODE", "catalog-id");

        // Act
        var result1 = await service.GetIdsByCodes("catalog-id", ["CODE"]);
        await service.DeleteAsync(["category-id"]);
        var result2 = await service.GetIdsByCodes("catalog-id", ["CODE"]);

        // Assert
        result1["CODE"].Should().Be("category-id");
        result2.Should().BeEmpty();
        service.GetIdsByCodesNoCacheCallsCount.Should().Be(2);
    }


    private static TestableCategoryService GetCategoryService(string categoryId, string categoryCode, string catalogId)
    {
        var categoryEntity = CreateCategoryEntity(categoryId, categoryCode, catalogId);
        var repository = GetCatalogRepository([categoryEntity]);

        return new TestableCategoryService(
            () => repository,
            GetPlatformMemoryCache(),
            Mock.Of<IEventPublisher>(),
            GetPropertiesValidator(),
            GetCatalogService(catalogId),
            Mock.Of<IOutlineService>(),
            Mock.Of<IBlobUrlResolver>(),
            Mock.Of<IPropertyValueSanitizer>());
    }

    private static CategoryEntity CreateCategoryEntity(string id, string code, string catalogId)
    {
        return new CategoryEntity
        {
            Id = id,
            Code = code,
            CatalogId = catalogId,
            Name = Guid.NewGuid().ToString(),
        };
    }

    private static Category CreateCategory(string id, string code, string catalogId)
    {
        return new Category
        {
            Id = id,
            Code = code,
            CatalogId = catalogId,
            Name = Guid.NewGuid().ToString(),
        };
    }

    private static ICatalogRepository GetCatalogRepository(List<CategoryEntity> categories)
    {
        var repositoryMock = new Mock<ICatalogRepository>();

        repositoryMock
            .Setup(x => x.Categories)
            .Returns(categories.BuildMockDbSet().Object);

        repositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IList<string> ids, string _) =>
            {
                return categories.Where(x => ids.Contains(x.Id)).ToList();
            });

        repositoryMock
            .Setup(x => x.RemoveCategoriesAsync(It.IsAny<IList<string>>()))
            .Callback((IList<string> ids) =>
            {
                categories.RemoveAll(x => ids.Contains(x.Id));
            });

        repositoryMock
            .Setup(x => x.CategoryLinks)
            .Returns(new List<CategoryRelationEntity>().BuildMockDbSet().Object);

        repositoryMock
            .Setup(x => x.Items)
            .Returns(new List<ItemEntity>().BuildMockDbSet().Object);

        repositoryMock
            .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync(new List<string>());

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
        public int GetIdsByCodesNoCacheCallsCount { get; private set; }

        protected override Task<IList<CategoryCodeCacheItem>> GetIdsByCodesNoCache(string catalogId, IList<string> codes)
        {
            GetIdsByCodesNoCacheCallsCount++;
            return base.GetIdsByCodesNoCache(catalogId, codes);
        }
    }
}
