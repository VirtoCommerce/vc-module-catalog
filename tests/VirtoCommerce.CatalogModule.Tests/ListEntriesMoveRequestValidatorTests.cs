using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ListEntriesMoveRequestValidatorTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock = new Mock<ICategoryService>();
        private readonly ListEntriesMoveRequestValidator _validator;

        public ListEntriesMoveRequestValidatorTests()
        {
            _validator = new ListEntriesMoveRequestValidator(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task Validate_NoTargetCategory_NotValid()
        {
            // Arrange
            var targetCateroryId = "targetCat";
            var movedCateroryId = "movedCat";

            var moveRequest = new ListEntriesMoveRequest()
            {
                Category = targetCateroryId,
                ListEntries = new[]
                {
                    new CategoryListEntry()
                    {
                        Type = CategoryListEntry.TypeName,
                        Id = movedCateroryId,
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
            var validationResult = await _validator.ValidateAsync(moveRequest);

            // Assert
            validationResult.IsValid.Should().BeTrue();
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
