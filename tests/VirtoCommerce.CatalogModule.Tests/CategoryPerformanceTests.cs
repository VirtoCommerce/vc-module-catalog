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
    public async Task Test()
    {
        // Arrange
        const int childCategoriesCount = 100;
        const int batchSize = 10;

        var catalogs = CreateCatalogs(2);
        var catalogId = catalogs.Last().Id;
        var allCategories = CreateCategories(catalogId, childCategoriesCount);

        var repositoryMock = GetCatalogRepositoryMock(catalogs, allCategories);
        var categorySearchService = GetCategorySearchService(repositoryMock);

        // Act
        var criteria = new CategorySearchCriteria { CatalogId = catalogId, Take = batchSize };
        var categories = await categorySearchService.SearchAllNoCloneAsync(criteria);

        // Assert
        categories.Should().NotBeNull();
        categories.Count.Should().Be(allCategories.Count);

        const int expectedRepositoryCalls = childCategoriesCount / batchSize
            + 1  // get parent2 and linked1
            + 1; // get parent1

        repositoryMock.Verify(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()), Times.Exactly(expectedRepositoryCalls));
    }


    private static CategorySearchService GetCategorySearchService(Mock<ICatalogRepository> repositoryMock)
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
            new Mock<IOutlineService>().Object,
            new Mock<IBlobUrlResolver>().Object,
            propertyValueSanitizerMock.Object);

        var crudOptions = Options.Create(new CrudOptions());
        var searchService = new CategorySearchService(repositoryFactory, platformMemoryCache, crudService, crudOptions);

        return searchService;
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

    private static List<CategoryEntity> CreateCategories(string catalogId, int count)
    {
        var categories = new List<CategoryEntity>();

        var parent1 = new CategoryEntity
        {
            Id = "parent1",
            CatalogId = catalogId,
            Name = "Parent 1",
        };

        var parent2 = new CategoryEntity
        {
            Id = "parent2",
            CatalogId = catalogId,
            Name = "Parent 2",
            ParentCategoryId = parent1.Id,
        };

        var linked1 = new CategoryEntity
        {
            Id = "linked1",
            CatalogId = catalogId,
            Name = "Linked 1",
            ParentCategoryId = parent1.Id,
        };

        for (var i = 1; i <= count; i++)
        {
            var number = i.ToString("0000");

            var category = new CategoryEntity
            {
                Id = $"category{i}",
                CatalogId = catalogId,
                Name = $"Category {number}",
                ParentCategoryId = parent2.Id,
                OutgoingLinks = [
                    new CategoryRelationEntity
                    {
                        TargetCatalogId = catalogId,
                        TargetCategoryId = linked1.Id,
                    }],
            };

            categories.Add(category);
        }

        categories.Add(parent1);
        categories.Add(parent2);
        categories.Add(linked1);

        return categories;
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
