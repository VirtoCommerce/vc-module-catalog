using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Validation;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ListEntriesMoveRequestValidatorTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly ListEntriesMoveRequestValidator _validator;

        public ListEntriesMoveRequestValidatorTests()
        {
            _validator = new ListEntriesMoveRequestValidator(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task Validate_NoTargetCategory_NotValid()
        {
            // Arrange
            var targetCategoryId = "targetCat";
            var movedCategoryId = "movedCat";

            var moveRequest = new ListEntriesMoveRequest
            {
                Category = targetCategoryId,
                ListEntries = new[]
                {
                    new CategoryListEntry
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCategoryId,
                    }
                },
            };

            // Act
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Destination category does not exist.");
        }

        [Theory]
        [InlineData("Catalog/movedCat/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/movedCat/*virtual/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/movedCat/child1/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/*virtual/movedCat/targetCat", "Catalog/*virtual/movedCat")]
        public async Task Validate_PasteUnderItself_NotValid(string targetCategoryPath, string movedCategoryPath)
        {
            // Arrange
            var targetCategoryId = targetCategoryPath.Split("/").Last();
            var movedCategoryId = movedCategoryPath.Split("/").Last();

            MockCategoryGetById(targetCategoryPath);

            var moveRequest = new ListEntriesMoveRequest
            {
                Category = targetCategoryId,
                ListEntries = new[]
                {
                    new CategoryListEntry
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCategoryId,
                        Outline = movedCategoryPath.Split("/")
                    }
                },
            };

            // Act
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Cannot move category under itself.");
        }

        [Theory]
        [InlineData("Catalog/movedCat2/targetCat", "Catalog/movedCat")]
        [InlineData("Catalog/anyOtherCat/targetCat", "Catalog/movedCat")]
        [InlineData("DifferentCatalog/movedCat/child1/targetCat", "Catalog/movedCat")]
        public async Task Validate_PasteNotUnderItself_Valid(string targetCategoryPath, string movedCategoryPath)
        {
            // Arrange
            var targetCategoryId = targetCategoryPath.Split("/").Last();
            var movedCategoryId = movedCategoryPath.Split("/").Last();

            MockCategoryGetById(targetCategoryPath);
            MockCategoryGetById(movedCategoryPath);

            var moveRequest = new ListEntriesMoveRequest
            {
                Category = targetCategoryId,
                ListEntries = new[]
                {
                    new CategoryListEntry
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCategoryId,
                        Outline = movedCategoryPath.Split("/")
                    }
                },
            };

            // Act
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            validationResult.IsValid.Should().BeTrue();
        }

        private void MockCategoryGetById(string path)
        {
            var parts = path.Split("/");
            var catalogId = parts.FirstOrDefault();
            var categoryId = parts.LastOrDefault();
            var outline = string.Join("/", parts.Skip(1));

            _categoryServiceMock
                .Setup(x => x.GetAsync(new[] { categoryId }, It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(() =>
                    {
                        // This complex part is done to allow Category.Outline property to be calculated. It uses Category.Parent recursively, so need to create all hierarchy.
                        var outlineParts = outline.Split("/").Reverse();
                        Category currentCategory = null, resultCategory = null;

                        foreach (var part in outlineParts)
                        {
                            if (currentCategory == null)
                            {
                                resultCategory = currentCategory = new Category
                                {
                                    CatalogId = catalogId,
                                    Id = part
                                };
                            }
                            else
                            {
                                currentCategory.Parent = new Category
                                {
                                    CatalogId = catalogId,
                                    Id = part
                                };
                                currentCategory = currentCategory.Parent;
                            }
                        }

                        return new[] { resultCategory };
                    }
                );
        }
    }
}
