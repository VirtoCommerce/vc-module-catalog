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
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CategoryInheritanceTests
    {
        private readonly Mock<ICatalogRepository> _repositoryMock = new();
        private readonly Mock<IEventPublisher> _eventPublisherMock = new();
        private readonly Mock<ICatalogService> _catalogServiceMock = new();
        private readonly Mock<IOutlineService> _outlineServiceMock = new();
        private readonly Mock<IBlobUrlResolver> _blobUrlResolverMock = new();
        private readonly Mock<AbstractValidator<IHasProperties>> _hasPropertyValidatorMock = new();
        private readonly Mock<IPropertyValueSanitizer> _propertyValueSanitizerMock = new();

        [Fact]
        public async Task GetAsync_CategoriesHaveProperties_ShouldInheritProperties()
        {
            // Arrange
            var catalogProperty = new Property
            {
                Id = "catalogProperty",
                Name = "catalogProperty",
            };

            var catalog = new Catalog
            {
                Id = "MainCatalog",
                Properties = new List<Property> { catalogProperty },
            };

            var rootProperty = new PropertyEntity
            {
                Id = "rootProperty",
                Name = "rootProperty",
                CatalogId = catalog.Id,
                CategoryId = "root",
            };
            var level1Property = new PropertyEntity
            {
                Id = "level1Property",
                Name = "level1Property",
                CatalogId = catalog.Id,
                CategoryId = "Level1",
            };
            var level2Property = new PropertyEntity
            {
                Id = "level2Property",
                Name = "level2Property",
                CatalogId = catalog.Id,
                CategoryId = "Level2",
            };

            var rootCategory = new CategoryEntity
            {
                Id = "root",
                CatalogId = catalog.Id,
                Properties = [rootProperty],
            };
            var level1Category = new CategoryEntity
            {
                Id = "Level1",
                ParentCategoryId = rootCategory.Id,
                CatalogId = catalog.Id,
                Properties = [level1Property],
            };
            var level2Category = new CategoryEntity
            {
                Id = "Level2",
                ParentCategoryId = level1Category.Id,
                CatalogId = catalog.Id,
                Properties = [level2Property],
            };

            var catalogs = new[] { catalog };
            var categoryEntities = new[] { rootCategory, level1Category, level2Category };

            _repositoryMock
                .Setup(x => x.GetCategoriesByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .ReturnsAsync((IList<string> ids, string _) => categoryEntities.Where(x => ids.Contains(x.Id)).ToArray());

            _catalogServiceMock
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((IList<string> ids, string _, bool _) => catalogs.Where(x => ids.Contains(x.Id)).ToArray());

            // Act
            var target = GetCategoryService();
            var categories = await target.GetAsync([level2Category.Id]);

            // Assert
            categories.Should().HaveCount(1);
            var category = categories.First();
            category.Id.Should().Be(level2Category.Id);
            category.Properties.Should().HaveCount(4);
            category.Properties.Should().Contain(x => x.Id == catalogProperty.Id);
            category.Properties.Should().Contain(x => x.Id == rootProperty.Id);
            category.Properties.Should().Contain(x => x.Id == level1Property.Id);
            category.Properties.Should().Contain(x => x.Id == level2Property.Id);
        }

        [Fact]
        public async Task GetAsync_CategoryIdIsNull_ShouldReturnEmptyResult()
        {
            // Act
            var target = GetCategoryService();
            var hierarchyResult = await target.GetAsync([null]);

            // Assert
            hierarchyResult.Should().HaveCount(0);
        }

        private CategoryService GetCategoryService()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            return GetCategoryService(platformMemoryCache, _repositoryMock.Object);
        }

        private CategoryService GetCategoryService(IPlatformMemoryCache platformMemoryCache, ICatalogRepository catalogRepository)
        {
            _hasPropertyValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), CancellationToken.None))
                .ReturnsAsync(new ValidationResult());

            return new CategoryService(() => catalogRepository,
                platformMemoryCache,
                _eventPublisherMock.Object,
                _hasPropertyValidatorMock.Object,
                _catalogServiceMock.Object,
                _outlineServiceMock.Object,
                _blobUrlResolverMock.Object,
                _propertyValueSanitizerMock.Object);
        }
    }
}
