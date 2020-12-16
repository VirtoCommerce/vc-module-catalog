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
        private readonly string _targetCateroryId = "targetCat";
        private readonly string _movedCateroryId = "movedCat";
        private readonly CategoryMover _categoryMover;

        public CategoryMoverTests()
        {
            _categoryMover = new CategoryMover(_categoryServiceMock.Object);
        }

        [Theory]
        [InlineData("Catalog/movedCat", "Catalog")]
        [InlineData("Catalog/movedCat/", "Catalog")]
        [InlineData("Catalog/movedCat/*virtual", "Catalog")]
        [InlineData("Catalog/movedCat/child1", "Catalog")]
        [InlineData("Catalog/*virtual/movedCat", "Catalog/*virtual")]
        public async Task PrepareMoveAsync_PasteUnderItself_Throws(string targetCategoryOutline, string movedCategoryOutline)
        {
            // Arrange
            MockCategoryGetById(targetCategoryOutline, _targetCateroryId);

            var moveRequest = new ListEntriesMoveRequest()
            {
                Category = _targetCateroryId,
                ListEntries = new[]
                {
                    new CategoryListEntry()
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = _movedCateroryId,
                        Outline = movedCategoryOutline.Split("/")
                    }
                },
            };

            // Act
            async Task action() => await _categoryMover.PrepareMoveAsync(moveRequest);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(action);
        }

        [Theory]
        [InlineData("Catalog/movedCat2", "Catalog")]
        [InlineData("Catalog/anyOtherCat", "Catalog")]
        [InlineData("DifferentCatalog/movedCat/child1", "Catalog")]
        public async Task PrepareMoveAsync_PasteNotUnderItself_Passes(string targetCategoryOutline, string movedCategoryOutline)
        {
            // Arrange
            MockCategoryGetById(targetCategoryOutline, _targetCateroryId);
            MockCategoryGetById(movedCategoryOutline, _movedCateroryId);

            var moveRequest = new ListEntriesMoveRequest()
            {
                Category = _targetCateroryId,
                ListEntries = new[]
                {
                    new CategoryListEntry()
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = _movedCateroryId,
                        Outline = movedCategoryOutline.Split("/")
                    }
                },
            };

            // Act
            var categories = await _categoryMover.PrepareMoveAsync(moveRequest);

            // Assert
            Assert.Single(categories);

        }

        private void MockCategoryGetById(string outlineString, string id)
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
                                Items = outlineString.Split("/", StringSplitOptions.RemoveEmptyEntries)
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
