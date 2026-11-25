using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.MCP;
using VirtoCommerce.MCP.Testing;
using Xunit;
using static VirtoCommerce.CatalogModule.Tests.MCP.CatalogToolsTestContext;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

[Trait("Category", "Unit")]
public class CatalogToolsetConventionsTests
{
    public static readonly string[] ReadTools =
    [
        "find_products_not_for_sale", "get_catalog_overview", "get_catalog_structure", "get_product", "get_virtual_catalog", "search_products",
    ];

    [Fact]
    public void Toolset_FollowsTheMcpConventions()
    {
        McpToolsetConventions.Verify<McpAssemblyMarker>();
    }

    [Fact]
    public async Task AManagerOfOneCatalog_SeesTheReadTools()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var visible = harness.GetVisibleToolNames(ManagerOf(CatalogToolsTestData.CatalogX));

        // Assert
        Assert.Equal(ReadTools, visible);
    }

    [Fact]
    public async Task AUserWithoutStoreRead_SeesOnlyTheCatalogTools()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var visible = harness.GetVisibleToolNames(McpTestUser.WithPermissions("catalog-reader", "catalog:read"));

        // Assert
        Assert.Equal(["get_catalog_structure", "get_virtual_catalog"], visible);
    }

    [Fact]
    public async Task AUserWithoutCatalogPermissions_SeesNoTool()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var visible = harness.GetVisibleToolNames(McpTestUser.WithPermissions("store-reader", "store:read"));

        // Assert
        Assert.Empty(visible);
    }
}
