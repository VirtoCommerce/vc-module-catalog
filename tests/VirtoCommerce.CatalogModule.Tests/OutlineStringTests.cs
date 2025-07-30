using VirtoCommerce.CatalogModule.Core.Outlines;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

public class OutlineStringTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("1", "1")]
    [InlineData("1/2", "2")]
    [InlineData("1/2/3", "3")]
    [InlineData("1___name1", "1")]
    [InlineData("1/2___name2", "2")]
    [InlineData("1/2/3___name3", "3")]
    [InlineData("1/2/3____name3", "3_")]
    public void GetLastItemId_ReturnsExpectedResult(string outline, string expectedId)
    {
        // Act
        var actualId = OutlineString.GetLastItemId(outline);

        // Assert
        Assert.Equal(expectedId, actualId);
    }
}
