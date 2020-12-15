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


        [Theory]
        [InlineData("Catalog/movedCat/", "Catalog")]
        [InlineData("Catalog/movedCat/*virtual", "Catalog")]
        [InlineData("Catalog/movedCat/child1", "Catalog")]
        public async Task PrepareMoveAsync_PasteUnderItself_Throws(string targetCategoryOutline, string movedCategoryOutline)
        {
            // Arrange
            var targetCateroryId = "targetCat";
            var movedCateroryId = "movedCat";

            MockCategoryGetById(targetCategoryOutline, targetCateroryId);

            var categoryMover = new CategoryMover(_categoryServiceMock.Object);

            var moveRequest = new ListEntriesMoveRequest()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new CategoryListEntry()
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCateroryId,
                        Outline = movedCategoryOutline.Split("/")
                    }
                },
            };

            // Act
            async Task action() => await categoryMover.PrepareMoveAsync(moveRequest);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(action);
        }

        [Fact]
        public async Task PrepareMoveAsync_PasteNotUnderItself_Passes()
        {
            // Arrange
            var categoryServiceStub = new Mock<ICategoryService>();
            var categoryMover = new CategoryMover(categoryServiceStub.Object);

            // Act

            // Assert

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
