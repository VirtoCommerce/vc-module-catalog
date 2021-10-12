using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CategoryInheritanceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICatalogRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ICatalogService> _catalogServiceMock;
        private readonly Mock<IOutlineService> _outlineServiceMock;
        private readonly Mock<IBlobUrlResolver> _blobUrlResolverMock;
        private readonly Mock<AbstractValidator<IHasProperties>> _hasPropertyValidatorMock;

        public CategoryInheritanceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<ICatalogRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _catalogServiceMock = new Mock<ICatalogService>();
            _outlineServiceMock = new Mock<IOutlineService>();
            _blobUrlResolverMock = new Mock<IBlobUrlResolver>();
            _hasPropertyValidatorMock = new Mock<AbstractValidator<IHasProperties>>();
        }

        [Fact]
        public async Task PreloadCategoryBranch_CategoriesHaveProperties_ShouldInheritProperties()
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
                Properties = new ObservableCollection<PropertyEntity> { rootProperty },
            };
            var level1Category = new CategoryEntity
            {
                Id = "Level1",
                ParentCategoryId = rootCategory.Id,
                CatalogId = catalog.Id,
                Properties = new ObservableCollection<PropertyEntity> { level1Property },
            };
            var level2Category = new CategoryEntity
            {
                Id = "Level2",
                ParentCategoryId = level1Category.Id,
                CatalogId = catalog.Id,
                Properties = new ObservableCollection<PropertyEntity> { level2Property },
            };

            var categoriesBranchResult = new List<CategoryEntity> { rootCategory, level1Category, level2Category };
            _repositoryMock.Setup(x => x.SearchCategoriesHierarchyAsync(level2Category.Id))
                .ReturnsAsync(categoriesBranchResult);

            _catalogServiceMock.Setup(t => t.GetByIdsAsync(new[] { catalog.Id }, It.IsAny<string>()))
                .ReturnsAsync(new[] { catalog });

            // Act
            var target = GetCategoryServiceWithPlatformMemoryCache();
            var hierarchyResult = await target.PreloadCategoryBranchAsyncStub(level2Category.Id);

            // Assert
            var category = hierarchyResult[level2Category.Id];
            category.Properties.Should().HaveCount(4);
            category.Properties.Should().Contain(x => x.Id == catalogProperty.Id);
            category.Properties.Should().Contain(x => x.Id == rootProperty.Id);
            category.Properties.Should().Contain(x => x.Id == level1Property.Id);
            category.Properties.Should().Contain(x => x.Id == level2Property.Id);
        }

        [Fact]
        public async Task PreloadCategoryBranch_CategoryIdIsNull_ShouldReturnEmptyResult()
        {
            // Act
            var target = GetCategoryServiceWithPlatformMemoryCache();
            var hierarchyResult = await target.PreloadCategoryBranchAsyncStub(null);

            // Assert
            hierarchyResult.Should().HaveCount(0);
        }

        private CategoryServiceStub GetCategoryServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetCategoryService(platformMemoryCache, _repositoryMock.Object);
        }

        private CategoryServiceStub GetCategoryService(IPlatformMemoryCache platformMemoryCache, ICatalogRepository catalogRepository)
        {
            _hasPropertyValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<IHasProperties>>(), default))
                .ReturnsAsync(new ValidationResult());

            return new CategoryServiceStub(() => catalogRepository,
                _eventPublisherMock.Object,
                platformMemoryCache,
                _hasPropertyValidatorMock.Object,
                _catalogServiceMock.Object,
                _outlineServiceMock.Object,
                _blobUrlResolverMock.Object);
        }


        public class CategoryServiceStub : CategoryService
        {
            public CategoryServiceStub(Func<ICatalogRepository> catalogRepositoryFactory,
                IEventPublisher eventPublisher,
                IPlatformMemoryCache platformMemoryCache,
                AbstractValidator<IHasProperties> hasPropertyValidator,
                ICatalogService catalogService,
                IOutlineService outlineService,
                IBlobUrlResolver blobUrlResolver) :
                base(catalogRepositoryFactory,
                    eventPublisher,
                    platformMemoryCache,
                    hasPropertyValidator,
                    catalogService,
                    outlineService,
                    blobUrlResolver)
            {
            }

            public async Task<IDictionary<string, Category>> PreloadCategoryBranchAsyncStub(string categoryId)
            {
                return await PreloadCategoryBranchAsync(categoryId);
            }
        }
    }
}
