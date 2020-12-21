using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "CI")]
    [CLSCompliant(false)]
    public class MoveInfoValidatorTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock = new Mock<ICategoryService>();
        private readonly MoveInfoValidator _validator;

        public MoveInfoValidatorTests()
        {
            _validator = new MoveInfoValidator(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task Validate_NoTargetCategory_NotValid()
        {
            // Arrange
            var targetCateroryId = "targetCat";
            var movedCateroryId = "movedCat";

            var moveRequest = new MoveInfo()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new ListEntryCategory(new Category()
                    {
                        Id = movedCateroryId,
                    })
                },
            };

            // Act
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.All(validationResult.Errors, item => Assert.Equal("Destination category does not exist.", item.ErrorMessage));
        }

        [Theory]
        [InlineData("Catalog/movedCat/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/movedCat/*virtual/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/movedCat/child1/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/*virtual/movedCat/targetCat", "Catalog/*virtual/movedCat")]
        public async Task Validate_PasteUnderItself_NotValid(string targetCategoryPath, string movedCategoryPath)
        {
            // Arrange
            var targetCateroryId = targetCategoryPath.Split('/').Last();
            var movedCategoryPathParts = movedCategoryPath.Split('/');
            var movedCateroryCatalogId = movedCategoryPathParts.First();
            var movedCategoryPathPartsWithoutCatalog = movedCategoryPathParts.Skip(1);
            var movedCateroryId = movedCategoryPathPartsWithoutCatalog.Last();
            var movedCateroryOutline = string.Join("/", movedCategoryPathPartsWithoutCatalog.Take(movedCategoryPathPartsWithoutCatalog.Count() - 1));

            MockCategoryGetById(targetCategoryPath);

            var moveRequest = new MoveInfo()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new ListEntryCategory(new Category()
                    {
                        Id = movedCateroryId,
                        Outline = movedCateroryOutline,
                        CatalogId = movedCateroryCatalogId
                    })
                },
            };

            // Act
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Collection(validationResult.Errors, item => Assert.Equal("Cannot move category under itself.", item.ErrorMessage));
        }

        [Theory]
        [InlineData("Catalog/movedCat2/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/anyOtherCat/targetCat", "Catalog/movedCat")]
        [InlineData("DifferentCatalog/movedCat/child1/targetCat", "Catalog/movedCat")]
        public async Task Validate_PasteNotUnderItself_Valid(string targetCategoryPath, string movedCategoryPath)
        {
            // Arrange
            var targetCateroryId = targetCategoryPath.Split('/').Last();
            var movedCategoryPathParts = movedCategoryPath.Split('/');
            var movedCateroryCatalogId = movedCategoryPathParts.First();
            var movedCategoryPathPartsWithoutCatalog = movedCategoryPathParts.Skip(1);
            var movedCateroryId = movedCategoryPathPartsWithoutCatalog.Last();
            var movedCateroryOutline = string.Join("/", movedCategoryPathPartsWithoutCatalog.Take(movedCategoryPathPartsWithoutCatalog.Count() - 1));


            MockCategoryGetById(targetCategoryPath);
            MockCategoryGetById(movedCategoryPath);

            var moveRequest = new MoveInfo()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new ListEntryCategory(new Category()
                    {
                        Id = movedCateroryId,
                        Outline = movedCateroryOutline,
                        CatalogId = movedCateroryCatalogId
                    })
                },
            };

            // Act
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        private void MockCategoryGetById(string path)
        {
            var parts = path.Split('/');
            var catalogId = parts.FirstOrDefault();
            var categoryId = parts.LastOrDefault();
            var outline = string.Join("/", parts.Skip(1));

            _categoryServiceMock
                .Setup(x => x.GetByIds(It.Is<string[]>(ids => ids.Length == 1 && ids[0].EqualsInvariant(categoryId)), It.IsAny<domain.CategoryResponseGroup>(), null))
                .Returns(() =>
                {
                    // This complex part is done to allow Category.Outline property to be calculated. It uses Category.Parent recursively, so need to create all hierarchy.
                    var outlineParts = outline.Split('/').ToList();
                    var result = new domain.Category()
                    {
                        CatalogId = catalogId,
                        Id = categoryId
                    };
                    var parents = new List<domain.Category>();

                    foreach (var part in outlineParts.Take(outlineParts.Count - 1))
                    {
                        parents.Add(new domain.Category()
                        {
                            CatalogId = catalogId,
                            Id = part
                        });
                    }

                    result.Parents = parents.ToArray();

                    return new[] { result };
                }
                );
        }
    }
}
