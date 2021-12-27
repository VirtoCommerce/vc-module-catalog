using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CategoryIteratorTests
    {
        private readonly Fixture _fixture = new Fixture();

        public CategoryIteratorTests()
        {
            _fixture.Register(() => _fixture.Build<Category>()
                .Without(x => x.Children)
                .Without(x => x.Parent)
                .Without(x => x.Properties)
                .Without(x => x.Catalog)
                .Without(x => x.Parents)
                .Without(x => x.Links)
                .Without(x => x.Outlines)
                .Create()
            );
        }

        [Fact]
        public async Task TestPaginationFetching()
        {
            // Arrange
            var fakeCategories = _fixture.CreateMany<Category>(61).ToList();

            var settingsManager = new Mock<ISettingsManager>();
            SetUseIndexedSearchSetting(settingsManager, true);

            var catalogRepositoryFactory = new Mock<Func<ICatalogRepository>>();
            var categoryIndexedSearchService = new Mock<ICategoryIndexedSearchService>();
            categoryIndexedSearchService.Setup(x => x.SearchAsync(It.IsAny<CategoryIndexedSearchCriteria>()))
                .ReturnsAsync(new CategoryIndexedSearchResult
                {
                    TotalCount = fakeCategories.Count,
                    Items = fakeCategories.ToArray(),
                });

            var iterator = new CategoryHierarchyIterator(catalogRepositoryFactory.Object, settingsManager.Object, categoryIndexedSearchService.Object, 60, "testCatalogId", "testCategoryId");

            // Act
            var categoryIdsFirstCall = await iterator.GetNextPageAsync();
            var categoryIdsSecondCall = await iterator.GetNextPageAsync();
            var categoryIdsThirdCall = await iterator.GetNextPageAsync();

            // Assert
            catalogRepositoryFactory.Verify(x => x.Invoke(), Times.Never);
            Assert.Equal(60, categoryIdsFirstCall.Count);
            Assert.Equal(1, categoryIdsSecondCall.Count);
            Assert.Equal(0, categoryIdsThirdCall.Count);
            Assert.False(iterator.HasMoreResults);
        }

        [Fact]
        public async Task TestAdditionalCategoriesLoading()
        {
            // Arrange
            var categories = _fixture.CreateMany<Category>(101).ToList();

            var settingsManager = new Mock<ISettingsManager>();
            SetUseIndexedSearchSetting(settingsManager, true);

            var catalogRepositoryFactory = new Mock<Func<ICatalogRepository>>();
            var categoryIndexedSearchService = new Mock<ICategoryIndexedSearchService>();
            categoryIndexedSearchService
                .SetupSequence(x => x.SearchAsync(It.IsAny<CategoryIndexedSearchCriteria>()))
                .ReturnsAsync(new CategoryIndexedSearchResult
                {
                    Items = categories.Take(50).ToArray(),
                    TotalCount = categories.Count,
                })
                .ReturnsAsync(new CategoryIndexedSearchResult
                {
                    Items = categories.Skip(50).Take(51).ToArray(),
                    TotalCount = categories.Count,
                });

            var pageSize = 51;
            var iterator = new CategoryHierarchyIterator(catalogRepositoryFactory.Object, settingsManager.Object, categoryIndexedSearchService.Object, pageSize, "testCatalogId", "testCategoryId");

            var categoryIds = new List<string>();
            var iterationCount = 0;

            // Act
            do
            {
                categoryIds.AddRange(await iterator.GetNextPageAsync());
                iterationCount++;

            } while (iterator.HasMoreResults);

            // Assert
            catalogRepositoryFactory.Verify(x => x.Invoke(), Times.Never);
            categoryIndexedSearchService.Verify(x => x.SearchAsync(It.IsAny<CategoryIndexedSearchCriteria>()), Times.Exactly(2));
            Assert.Equal(categories.Count, categoryIds.Count);
            Assert.Equal(categories.First().Id, categoryIds.First());
            Assert.Equal(categories.Last().Id, categoryIds.Last());
            Assert.Equal(2, iterationCount);
        }

        [Fact]
        public async Task IndexedSearchDisabledTest()
        {
            // Arrange
            var settingsManager = new Mock<ISettingsManager>();
            SetUseIndexedSearchSetting(settingsManager, false);

            var categoryIndexedSearchService = new Mock<ICategoryIndexedSearchService>();

            var categoryId = "testCategoryId";
            var resultCategoryIds = _fixture.CreateMany<string>(100).ToArray();
            var catalogRepositoryFactory = new Mock<Func<ICatalogRepository>>();
            catalogRepositoryFactory.Setup(x => x.Invoke()).Returns(() =>
                {
                    var catalogRepository = new Mock<ICatalogRepository>();

                    catalogRepository
                        .Setup(x => x.GetAllChildrenCategoriesIdsAsync(new[] { categoryId }))
                        .ReturnsAsync(resultCategoryIds);

                    return catalogRepository.Object;
                }
                );

            var pageSize = 50;
            var iterator = new CategoryHierarchyIterator(catalogRepositoryFactory.Object, settingsManager.Object, categoryIndexedSearchService.Object, pageSize, "testCatalogId", categoryId);
            var categoryIds = new List<string>();
            var interationCount = 0;

            // Act
            do
            {
                categoryIds.AddRange(await iterator.GetNextPageAsync());
                interationCount++;

            } while (iterator.HasMoreResults);

            // Assert
            categoryIndexedSearchService.Verify(x => x.SearchAsync(It.IsAny<CategoryIndexedSearchCriteria>()), Times.Never);
            catalogRepositoryFactory.Verify(x => x.Invoke(), Times.Once);
            Assert.Equal(resultCategoryIds.Length, categoryIds.Count);
            Assert.Equal(2, interationCount);
        }

        [Fact]
        public async Task PageSizeTest()
        {
            // Arrange
            var settingsManager = new Mock<ISettingsManager>();
            SetUseIndexedSearchSetting(settingsManager, false);

            var resultCategoryIds = _fixture.CreateMany<string>(60);
            var catalogRepositoryFactory = new Mock<Func<ICatalogRepository>>();
            catalogRepositoryFactory.Setup(x => x.Invoke()).Returns(() =>
            {
                var catalogRepository = new Mock<ICatalogRepository>();
                catalogRepository
                    .Setup(x => x.GetAllChildrenCategoriesIdsAsync(It.IsAny<string[]>()))
                    .ReturnsAsync(resultCategoryIds.ToArray());

                return catalogRepository.Object;
            });

            var categoryIndexedSearchService = new Mock<ICategoryIndexedSearchService>();

            var pageSize = 20;
            var iterator = new CategoryHierarchyIterator(catalogRepositoryFactory.Object, settingsManager.Object, categoryIndexedSearchService.Object, pageSize, "testCatalogId", "testCategoryId");
            var defaultPageSize = 50;

            // Act
            var categoryIds = await iterator.GetNextPageAsync();

            // Assert
            Assert.Equal(defaultPageSize, categoryIds.Count);
        }

        [Theory]
        [InlineData("testCatalogId", "testCategoryId", "testCatalogId/testCategoryId")]
        [InlineData("testCatalogId", null, "testCatalogId")]
        public async Task OutlineTest(string catalogId, string categoryId, string expectedOutline)
        {
            // Arrange
            var settingsManager = new Mock<ISettingsManager>();
            SetUseIndexedSearchSetting(settingsManager, true);

            var catalogRepositoryFactory = new Mock<Func<ICatalogRepository>>();

            var categoryIndexedSearchService = new Mock<ICategoryIndexedSearchService>();
            var actualOutline = string.Empty;

            categoryIndexedSearchService
                .Setup(x => x.SearchAsync(It.IsAny<CategoryIndexedSearchCriteria>()))
                .Callback((CategoryIndexedSearchCriteria criteria) =>
                {
                    actualOutline = criteria.Outline;
                })
                .ReturnsAsync(new CategoryIndexedSearchResult
                {
                    TotalCount = 10,
                    Items = _fixture.CreateMany<Category>(10).ToArray(),
                });

            var iterator = new CategoryHierarchyIterator(catalogRepositoryFactory.Object, settingsManager.Object, categoryIndexedSearchService.Object, 50, catalogId, categoryId);

            // Act
            await iterator.GetNextPageAsync();

            // Assert
            Assert.Equal(expectedOutline, actualOutline);
        }

        [Fact]
        public async Task IndexSearchDisabledAndCategoryIdIsNullOrEmpty()
        {
            // Arrange
            var settingsManager = new Mock<ISettingsManager>();
            SetUseIndexedSearchSetting(settingsManager, false);

            var catalogRepositoryFactory = new Mock<Func<ICatalogRepository>>();
            var categoryIndexedSearchService = new Mock<ICategoryIndexedSearchService>();

            var iterator = new CategoryHierarchyIterator(catalogRepositoryFactory.Object, settingsManager.Object, categoryIndexedSearchService.Object, 50, "testCatalogId", null);

            // Act
            await iterator.GetNextPageAsync();

            // Assert
            catalogRepositoryFactory.Verify(x => x.Invoke(), Times.Never);
            categoryIndexedSearchService.Verify(x => x.SearchAsync(It.IsAny<CategoryIndexedSearchCriteria>()), Times.Never);
            Assert.False(iterator.HasMoreResults);
        }


        private void SetUseIndexedSearchSetting(Mock<ISettingsManager> settingsManager, bool settingState)
        {
            settingsManager
                .Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry
                {
                    Value = settingState,
                });
        }
    }
}
