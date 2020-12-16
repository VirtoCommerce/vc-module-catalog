using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CategoryMoverTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock = new Mock<ICategoryService>();
        private readonly CategoryMover _categoryMover;

        public CategoryMoverTests()
        {
            _categoryMover = new CategoryMover(_categoryServiceMock.Object);
        }

        [Theory]
        [InlineData("Catalog/movedCat/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/movedCat/*virtual/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/movedCat/child1/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/*virtual/movedCat/targetCat", "Catalog/*virtual/movedCat")]
        public async Task PrepareMoveAsync_PasteUnderItself_Throws(string targetCategoryPath, string movedCategoryPath)
        {
            // Arrange
            var targetCateroryId = targetCategoryPath.Split("/").Last();
            var movedCateroryId = movedCategoryPath.Split("/").Last();

            MockCategoryGetById(targetCategoryPath, targetCateroryId);

            var moveRequest = new ListEntriesMoveRequest()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new CategoryListEntry()
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCateroryId,
                        Outline = movedCategoryPath.Split("/")
                    }
                },
            };

            // Act
            async Task action() => await _categoryMover.PrepareMoveAsync(moveRequest);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(action);
        }

        [Theory]
        [InlineData("Catalog/movedCat2/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/anyOtherCat/targetCat", "Catalog/movedCat")]
        [InlineData("DifferentCatalog/movedCat/child1/targetCat", "Catalog/movedCat")]
        public async Task PrepareMoveAsync_PasteNotUnderItself_Passes(string targetCategoryPath, string movedCategoryPath)
        {
            // Arrange
            var targetCateroryId = targetCategoryPath.Split("/").Last();
            var movedCateroryId = movedCategoryPath.Split("/").Last();

            MockCategoryGetById(targetCategoryPath, targetCateroryId);
            MockCategoryGetById(movedCategoryPath, movedCateroryId);

            var moveRequest = new ListEntriesMoveRequest()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new CategoryListEntry()
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCateroryId,
                        Outline = movedCategoryPath.Split("/")
                    }
                },
            };

            // Act
            var categories = await _categoryMover.PrepareMoveAsync(moveRequest);

            // Assert
            Assert.Single(categories);

        }

        private void MockCategoryGetById(string path, string id)
        {
            _categoryServiceMock
                .Setup(x => x.GetByIdsAsync(It.Is<string[]>(ids => ids.Length == 1 && ids[0].EqualsInvariant(id)), It.IsAny<string>(), null))
                .Returns(Task.FromResult(new[] {
                    new Category()
                    {
                        Outlines = new []
                        {
                            new Outline()
                            {
                                Items = path.Split("/", StringSplitOptions.RemoveEmptyEntries)
                                    .Select( part =>
                                        new OutlineItem()
                                        {
                                            Id = part
                                        })
                                    .ToList()
                            }
                        }
                    }
                }));
        }
    }
}
