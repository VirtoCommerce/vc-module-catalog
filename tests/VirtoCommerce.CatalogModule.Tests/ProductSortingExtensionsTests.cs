using System.Collections.Generic;
using FluentAssertions;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class ProductSortingExtensionsTests
    {
        private static List<ProductSorting> Sample() =>
        [
            new() { Code = "featured", IsDefault = true, IsVisible = true },
            new() { Code = "name-ascending", IsVisible = true },
            new() { Code = "price-ascending", IsVisible = false },
        ];

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void FindSelected_EmptySort_ReturnsDefault(string sort)
        {
            var result = Sample().FindSelected(sort);

            result.Should().NotBeNull();
            result.Code.Should().Be("featured");
        }

        [Theory]
        [InlineData("name-ascending")]
        [InlineData("NAME-ASCENDING")] // code match is case-insensitive
        public void FindSelected_KnownCode_ReturnsThatOrdering(string sort)
        {
            Sample().FindSelected(sort).Code.Should().Be("name-ascending");
        }

        [Theory]
        [InlineData("rating:desc")]       // raw expression
        [InlineData("nonexistent-code")]  // unknown code
        public void FindSelected_RawOrUnknownToken_ReturnsNull(string sort)
        {
            Sample().FindSelected(sort).Should().BeNull();
        }

        [Fact]
        public void FindSelected_EmptySort_NoDefaultFlag_FallsBackToFirstVisible()
        {
            var list = new List<ProductSorting>
            {
                new() { Code = "hidden", IsVisible = false },
                new() { Code = "visible", IsVisible = true },
            };

            list.FindSelected("").Code.Should().Be("visible");
        }
    }
}
