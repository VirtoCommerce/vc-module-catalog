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
using VirtoCommerce.CatalogModule.Core.Model.OutlinePart;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

public class CategoryPerformanceTests
{
    [Fact]
    public async Task SearchAll_ShouldLoadCategoriesInBatches()
    {
        // Arrange
        const int catalogsCount = 2;
        const int childCategoriesCount = 100;
        const int batchSize = 10;

        var catalogs = CreateCatalogs(catalogsCount);
        var allCategories = CreateCategories(catalogs, childCategoriesCount);

        var repositoryMock = GetCatalogRepositoryMock(catalogs, allCategories);
        var (_, searchService) = GetCategoryServices(repositoryMock);

        var catalogId = catalogs.Last().Id;
        var expectedCategoriesCount = allCategories.Count(x => x.CatalogId == catalogId);

        const int expectedRepositoryCallCount = childCategoriesCount / batchSize
            + 1  // get parent2 and linked1
            + 1; // get parent1

        // Act
        var criteria = new CategorySearchCriteria { CatalogId = catalogId, Take = batchSize };
        var categories1 = await searchService.SearchAllNoCloneAsync(criteria);
        var categories2 = await searchService.SearchAllNoCloneAsync(criteria);

        // Assert
        categories1.Should().NotBeNull();
        categories2.Should().NotBeNull();
        categories1.Count.Should().Be(expectedCategoriesCount);
        categories2.Count.Should().Be(expectedCategoriesCount);
        repositoryMock.Verify(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Exactly(expectedRepositoryCallCount));
    }

    [Fact]
    public async Task GetAsync_WithUnknownId_ShouldReturnNull()
    {
        // Arrange
        var repositoryMock = GetCatalogRepositoryMock(catalogs: [], categories: []);
        var (crudService, _) = GetCategoryServices(repositoryMock);

        // Act
        var category1 = await crudService.GetNoCloneAsync("unknown_id"); // First call should access the repository
        var category2 = await crudService.GetNoCloneAsync("unknown_id"); // Second call should get data from the cache

        // Assert
        category1.Should().BeNull();
        category2.Should().BeNull();
        repositoryMock.Verify(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Once());
    }


    private static List<CatalogEntity> CreateCatalogs(int count)
    {
        var catalogs = new List<CatalogEntity>();

        for (var i = 1; i <= count; i++)
        {
            var catalog = new CatalogEntity
            {
                Id = $"catalog{i}",
                Name = $"Catalog {i}",
            };

            catalogs.Add(catalog);
        }

        return catalogs;
    }

    private static List<CategoryEntity> CreateCategories(List<CatalogEntity> catalogs, int count)
    {
        var categories = new List<CategoryEntity>();

        foreach (var catalog in catalogs)
        {
            var catalogId = catalog.Id;

            var parent1 = AddCategory(categories, "parent1", "Parent 1", catalogId);
            var parent2 = AddCategory(categories, "parent2", "Parent 2", catalogId, parent1.Id);
            var linked1 = AddCategory(categories, "linked1", "Linked 1", catalogId, parent1.Id);

            for (var i = 1; i <= count; i++)
            {
                AddCategory(categories, $"category{i}", $"Category {i:0000}", catalogId, parent2.Id, linked1.Id);
            }
        }

        return categories;
    }

    private static CategoryEntity AddCategory(List<CategoryEntity> categories, string id, string name, string catalogId, string parentCategoryId = null, string targetCategoryId = null)
    {
        var category = new CategoryEntity
        {
            Id = $"{catalogId}_{id}",
            Name = name,
            CatalogId = catalogId,
            ParentCategoryId = parentCategoryId,
        };

        if (targetCategoryId != null)
        {
            category.OutgoingLinks = [
                new CategoryRelationEntity
                {
                    TargetCatalogId = catalogId,
                    TargetCategoryId = targetCategoryId,
                },
            ];
        }

        categories.Add(category);

        return category;
    }

    private static Mock<ICatalogRepository> GetCatalogRepositoryMock(List<CatalogEntity> catalogs, List<CategoryEntity> categories)
    {
        var repositoryMock = new Mock<ICatalogRepository>();

        var catalogsDbSetMock = catalogs.BuildMockDbSet();
        repositoryMock.Setup(x => x.Catalogs).Returns(catalogsDbSetMock.Object);

        repositoryMock
            .Setup(x => x.GetCatalogsByIdsAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync((IList<string> ids) =>
            {
                return catalogs.Where(x => ids.Contains(x.Id)).ToList();
            });

        var categoriesDbSetMock = categories.BuildMockDbSet();
        repositoryMock.Setup(x => x.Categories).Returns(categoriesDbSetMock.Object);

        repositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IList<string> ids, string _) =>
            {
                return categories.Where(x => ids.Contains(x.Id)).ToList();
            });

        return repositoryMock;
    }

    private static (CategoryService, CategorySearchService) GetCategoryServices(Mock<ICatalogRepository> repositoryMock)
    {
        var repositoryFactory = () => repositoryMock.Object;

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

        var eventPublisherMock = new Mock<IEventPublisher>();

        var propertyValidatorMock = new Mock<AbstractValidator<IHasProperties>>();

        propertyValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), CancellationToken.None))
            .ReturnsAsync(new ValidationResult());

        var propertyValueSanitizerMock = new Mock<IPropertyValueSanitizer>();

        var crudService = new CategoryService(
            repositoryFactory,
            platformMemoryCache,
            eventPublisherMock.Object,
            propertyValidatorMock.Object,
            GetCatalogService(repositoryFactory, eventPublisherMock, platformMemoryCache, propertyValidatorMock, propertyValueSanitizerMock),
            new OutlineService(new NameOutlinePartResolver(), new IdOutlinePartResolver()),
            new Mock<IBlobUrlResolver>().Object,
            propertyValueSanitizerMock.Object);

        var crudOptions = Options.Create(new CrudOptions());
        var searchService = new CategorySearchService(repositoryFactory, platformMemoryCache, crudService, crudOptions);

        return (crudService, searchService);
    }

    private static CatalogService GetCatalogService(
        Func<ICatalogRepository> repositoryFactory,
        Mock<IEventPublisher> eventPublisherMock,
        PlatformMemoryCache platformMemoryCache,
        Mock<AbstractValidator<IHasProperties>> propertyValidatorMock,
        Mock<IPropertyValueSanitizer> propertyValueSanitizerMock)
    {
        return new CatalogService(
            repositoryFactory,
            eventPublisherMock.Object,
            platformMemoryCache,
            propertyValidatorMock.Object,
            propertyValueSanitizerMock.Object);
    }
}
