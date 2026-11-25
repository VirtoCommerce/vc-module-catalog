using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.MCP.Testing;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;
using static VirtoCommerce.CatalogModule.Tests.MCP.CatalogToolsTestContext;
using static VirtoCommerce.CatalogModule.Tests.MCP.CatalogToolsTestData;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

// The access matrix: A has the catalog permissions globally, B only for the physical catalog X, C only for the virtual
// catalog V. Every answer of B and C is searched for the marker that every name, SKU and id of the catalog Y carries.
[Trait("Category", "Unit")]
public class CatalogToolsAccessTests
{
    public const string OtherCatalogsNote = "This store also sells products of catalogs you may not read: they are not shown or counted.";

    [Fact]
    public async Task SearchProducts_GlobalManager_ListsEveryCatalogOfTheVirtualStore()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, GlobalManager(), "search_products", new { store_id = StoreV });

        // Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal(["LEAK-Y-100", "X-100", "X-200"], GetSkus(json.GetProperty("products")));
        Assert.True(IsLeftOut(json, "other_catalogs"));
    }

    // The store's catalog id has capitals; the index keeps outlines in lower case.
    [Fact]
    public async Task SearchProducts_GlobalManager_FindsTheProductsOfACatalogIdWithCapitals()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, GlobalManager(), "search_products", new { store_id = StoreY });

        // Assert
        Assert.Equal(["LEAK-Y-100"], GetSkus(result.EnsureSucceeded().Json.GetProperty("products")));
    }

    // store:read limited to the store X: the other stores are refused, and the overview lists the store X only.
    [Fact]
    public async Task AManagerOfStoreX_WorksOnTheStoreXOnly()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());
        var user = McpTestUser.Create("store-x-manager")
            .WithPermissions(CatalogReadPermissions)
            .WithScopedPermission("store:read", "StoreSelectedScope", StoreX);

        // Act
        var otherStore = await CallAsync(harness, user, "search_products", new { store_id = StoreV });
        var ownStore = await CallAsync(harness, user, "search_products", new { store_id = StoreX });
        var overview = await CallAsync(harness, user, "get_catalog_overview");

        // Assert
        Assert.Equal(McpToolCallOutcome.Forbidden, otherStore.Outcome);
        Assert.Equal(["X-100", "X-200"], GetSkus(ownStore.EnsureSucceeded().Json.GetProperty("products")));
        Assert.Equal([StoreX], overview.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray().Select(x => x.GetProperty("store_id").GetString()));
    }

    [Fact]
    public async Task SearchProducts_ManagerOfX_ListsOnlyXInTheVirtualStore()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreV });

        // Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal(["X-100", "X-200"], GetSkus(json.GetProperty("products")));
        Assert.Equal(2, json.GetProperty("products").GetProperty("total_count").GetInt32());
        Assert.Equal(OtherCatalogsNote, json.GetProperty("other_catalogs").GetString());
        AssertNoLeak(result);
    }

    [Fact]
    public async Task SearchProducts_ManagerOfX_FindsNothingInTheStoreOfY()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreY, keyword = "Drill" });

        // Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Empty(GetSkus(json.GetProperty("products")));
        Assert.Equal(0, json.GetProperty("products").GetProperty("total_count").GetInt32());
        AssertNoLeak(result);
    }

    [Fact]
    public async Task GetProduct_ManagerOfX_GetsTheAnswerOfAnUnknownSkuForASkuOfY()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var ofY = await CallAsync(harness, ManagerOf(CatalogX), "get_product", new { store_id = StoreV, sku = "LEAK-Y-100" });
        var unknown = await CallAsync(harness, ManagerOf(CatalogX), "get_product", new { store_id = StoreV, sku = "NOPE-1" });

        // Assert
        Assert.False(ofY.Succeeded);
        Assert.Equal(unknown.Outcome, ofY.Outcome);
        Assert.Equal(unknown.Error, ofY.Error.Replace("LEAK-Y-100", "NOPE-1"));
    }

    [Fact]
    public async Task GetProduct_ManagerOfX_GetsAProductOfXInTheVirtualStore()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "get_product", new { store_id = StoreV, sku = "X-100" });

        // Assert
        var json = result.EnsureSucceeded().Json;
        var product = json.GetProperty("product");
        Assert.Equal("X-100", product.GetProperty("sku").GetString());
        Assert.Equal("Office > Printers > Laser", product.GetProperty("category").GetString());
        Assert.True(json.GetProperty("can_be_bought").GetBoolean());
        Assert.True(json.GetProperty("in_search_index").GetBoolean());
        AssertNoLeak(result);
    }

    [Fact]
    public async Task FindProductsNotForSale_ManagerOfX_ChecksTheWindowOfX()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "find_products_not_for_sale", new { store_id = StoreV });

        // Assert
        var json = result.EnsureSucceeded().Json;
        var notForSale = Assert.Single(json.GetProperty("not_for_sale").EnumerateArray());
        Assert.Equal("X-200", notForSale.GetProperty("sku").GetString());
        Assert.Contains(notForSale.GetProperty("reasons").EnumerateArray(), x => x.GetProperty("id").GetString() == "product-inactive");
        Assert.Equal(1, json.GetProperty("for_sale").GetInt32());
        Assert.Equal(1, json.GetProperty("store_wide").GetProperty("hidden").GetInt32());
        AssertNoLeak(result);
    }

    [Fact]
    public async Task FindProductsNotForSale_ManagerOfX_GetsTheAnswerOfAnUnknownSkuForASkuOfY()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "find_products_not_for_sale", new { store_id = StoreV, skus = new[] { "LEAK-Y-100", "NOPE-1" } });

        // Assert
        var notFound = result.EnsureSucceeded().Json.GetProperty("not_found").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Equal(2, notFound.Count);
        Assert.Equal(notFound[1], notFound[0].Replace("LEAK-Y-100", "NOPE-1"));
    }

    [Fact]
    public async Task GetCatalogOverview_ManagerOfX_CountsAndNamesOnlyX()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_overview");

        // Assert
        var stores = result.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray().ToDictionary(x => x.GetProperty("store_id").GetString());
        Assert.Equal([StoreV, StoreX, StoreY], stores.Keys.Order());

        var storeV = stores[StoreV];
        Assert.True(IsLeftOut(storeV, "catalog"));
        Assert.Equal("Office X", Assert.Single(storeV.GetProperty("sells_from").EnumerateArray()).GetProperty("name").GetString());
        Assert.Equal("Store V, Store X", storeV.GetProperty("sells_from")[0].GetProperty("sold_by").GetString());
        Assert.Equal(OtherCatalogsNote, storeV.GetProperty("other_catalogs").GetString());
        Assert.Equal((2, 1, 1, 0), GetFigures(storeV));

        var storeY = stores[StoreY];
        Assert.Empty(storeY.GetProperty("sells_from").EnumerateArray());
        Assert.Equal(OtherCatalogsNote, storeY.GetProperty("other_catalogs").GetString());
        Assert.Equal((0, 0, 0, 0), GetFigures(storeY));
        AssertNoLeak(result);
    }

    [Fact]
    public async Task GetCatalogOverview_GlobalManager_CountsEveryCatalogOfTheVirtualStore()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, GlobalManager(), "get_catalog_overview", new { store_id = StoreV });

        // Assert
        var store = Assert.Single(result.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray());
        Assert.Equal("Virtual V", store.GetProperty("catalog").GetProperty("name").GetString());
        Assert.Equal(["LEAK-Y Catalog", "Office X"], store.GetProperty("sells_from").EnumerateArray().Select(x => x.GetProperty("name").GetString()));
        Assert.Equal((3, 2, 1, 0), GetFigures(store));
    }

    [Fact]
    public async Task GetCatalogStructure_ManagerOfX_GetsTheAnswerOfAnUnknownCatalogForY()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var ofY = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_structure", new { catalog_id = CatalogY });
        var unknown = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_structure", new { catalog_id = "nope-catalog" });

        // Assert
        Assert.False(ofY.Succeeded);
        Assert.Equal(unknown.Error, ofY.Error.Replace(CatalogY, "nope-catalog"));
    }

    [Fact]
    public async Task GetCatalogStructure_ManagerOfX_ShowsTheTreeOfX()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_structure", new { catalog_id = CatalogX });

        // Assert
        var json = result.EnsureSucceeded().Json;
        Assert.Equal(["Printers", "Printers > Laser"], json.GetProperty("categories").EnumerateArray().Select(x => x.GetProperty("path").GetString()));
        Assert.Equal("Store V, Store X", json.GetProperty("sold_by").GetString());
        AssertNoLeak(result);
    }

    [Fact]
    public async Task GetVirtualCatalog_ManagerOfV_SeesItsOwnCategoriesWithoutTheLinksItMayNotRead()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogV), "get_virtual_catalog", new { catalog_id = CatalogV });

        // Assert
        var json = result.EnsureSucceeded().Json;
        var office = Assert.Single(json.GetProperty("categories").EnumerateArray());
        Assert.Equal("Office", office.GetProperty("name").GetString());
        Assert.Empty(office.GetProperty("links").EnumerateArray());
        Assert.Equal("Some linked categories or products belong to catalogs you may not read: they are not shown.",
            json.GetProperty("links_left_out").GetString());
        AssertNoLeak(result);
    }

    [Fact]
    public async Task GetVirtualCatalog_GlobalManager_SeesEveryLinkWithItsProducts()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var result = await CallAsync(harness, GlobalManager(), "get_virtual_catalog", new { catalog_id = CatalogV });

        // Assert
        var office = Assert.Single(result.EnsureSucceeded().Json.GetProperty("categories").EnumerateArray());
        var links = office.GetProperty("links").EnumerateArray()
            .Select(x => (x.GetProperty("catalog").GetString(), x.GetProperty("path").GetString(), x.GetProperty("products").GetInt32()))
            .OrderBy(x => x.Item1)
            .ToList();
        Assert.Equal([("LEAK-Y Catalog", "LEAK-Y Tools", 1), ("Office X", "Printers", 2)], links);
    }

    [Fact]
    public async Task ManagerOfV_SeesNoProductOfTheVirtualStore()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var search = await CallAsync(harness, ManagerOf(CatalogV), "search_products", new { store_id = StoreV });
        var overview = await CallAsync(harness, ManagerOf(CatalogV), "get_catalog_overview", new { store_id = StoreV });
        var window = await CallAsync(harness, ManagerOf(CatalogV), "find_products_not_for_sale", new { store_id = StoreV });

        // Assert
        Assert.Empty(GetSkus(search.EnsureSucceeded().Json.GetProperty("products")));
        var store = Assert.Single(overview.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray());
        Assert.Equal("Virtual V", store.GetProperty("catalog").GetProperty("name").GetString());
        Assert.Equal(OtherCatalogsNote, store.GetProperty("other_catalogs").GetString());
        Assert.Equal(0, window.EnsureSucceeded().Json.GetProperty("for_sale").GetInt32());

        foreach (var result in new[] { search, overview, window })
        {
            AssertNoLeak(result);
        }
    }

    // A category of X linked into Y: X's products are listed under Y too, so the index queries that leave Y's products out
    // leave them out; the figures that would fall short are left out, with the reason.
    [Fact]
    public async Task ManagerOfX_GetsNoFigureThatWouldFallShort()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.CategoryLinks.Add(new CategoryLink
        {
            ListEntryId = PrintersX, ListEntryType = "Category", CatalogId = CatalogY, CategoryId = ToolsY,
        });
        await using var harness = CreateHarness(data);

        // Act
        var overview = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_overview", new { store_id = StoreV });
        var search = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreV });

        // Assert
        var store = Assert.Single(overview.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray());
        Assert.True(IsLeftOut(store.GetProperty("products"), "in_search_index"));
        Assert.StartsWith("Some products of these catalogs are also linked into catalogs you may not read", store.GetProperty("products").GetProperty("not_counted").GetString());
        Assert.False(IsLeftOut(search.EnsureSucceeded().Json, "not_exact"));
        AssertNoLeak(overview);
        AssertNoLeak(search);
    }

    // A category of Y linked into X: the index lists Y's products under X too, and they stay out of B's lists and figures.
    [Fact]
    public async Task ManagerOfX_DoesNotSeeProductsOfYLinkedIntoX()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.CategoryLinks.Add(new CategoryLink { ListEntryId = ToolsY, ListEntryType = "Category", CatalogId = CatalogX, CategoryId = PrintersX });
        await using var harness = CreateHarness(data);

        // Act
        var search = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreX });
        var overview = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_overview", new { store_id = StoreX });
        var window = await CallAsync(harness, ManagerOf(CatalogX), "find_products_not_for_sale", new { store_id = StoreX });

        // Assert
        Assert.Equal(["X-100", "X-200"], GetSkus(search.EnsureSucceeded().Json.GetProperty("products")));
        Assert.Equal((2, 1, 1, 0), GetFigures(Assert.Single(overview.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray())));
        Assert.Equal(1, window.EnsureSucceeded().Json.GetProperty("for_sale").GetInt32());

        foreach (var result in new[] { search, overview, window })
        {
            AssertNoLeak(result);
        }
    }

    // B may not read the virtual catalog V: its own categories stay hidden, and a category of X opens by its id.
    [Fact]
    public async Task SearchProducts_ManagerOfX_FiltersByCategoriesOfXOnly()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());

        // Act
        var byOwnPath = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreV, category = "Office" });
        var byIdOfX = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreV, category = LaserX });

        // Assert
        Assert.False(byOwnPath.Succeeded);
        Assert.DoesNotContain("Virtual V", byOwnPath.Error);
        Assert.Equal(["X-100"], GetSkus(byIdOfX.EnsureSucceeded().Json.GetProperty("products")));
    }

    // A product moved from X to Y that the index has not caught up with yet: the index still lists it under X, and the check
    // of each listed product on its own catalog keeps it out.
    [Fact]
    public async Task ManagerOfX_NeverSeesAProductTheIndexStillListsUnderX()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        var printer = data.GetProduct(LaserPrinter);
        data.IndexedDocuments[LaserPrinter] = data.GetIndexDocument(printer);
        printer.CatalogId = CatalogY;
        printer.CategoryId = ToolsY;
        printer.Name = "LEAK-Y moved printer";
        await using var harness = CreateHarness(data);

        // Act
        var search = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreX });
        var window = await CallAsync(harness, ManagerOf(CatalogX), "find_products_not_for_sale", new { store_id = StoreX });

        // Assert
        Assert.Equal(["X-200"], GetSkus(search.EnsureSucceeded().Json.GetProperty("products")));
        Assert.Equal(0, window.EnsureSucceeded().Json.GetProperty("for_sale").GetInt32());
        AssertNoLeak(search);
        AssertNoLeak(window);
    }

    // A product of a catalog whose link into the store's catalog is gone, which the index has not caught up with yet: the
    // index queries ask for the readable catalogs, so it is neither listed nor counted.
    [Fact]
    public async Task ManagerOfX_DoesNotCountAProductTheIndexStillListsUnderTheStore()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        var part = AddCatalogW(data);
        data.CategoryLinks.Add(new CategoryLink { ListEntryId = part.CategoryId, ListEntryType = "Category", CatalogId = CatalogV, CategoryId = OfficeV });
        data.IndexedDocuments[part.Id] = data.GetIndexDocument(part);
        data.CategoryLinks.RemoveAt(data.CategoryLinks.Count - 1);
        await using var harness = CreateHarness(data);

        // Act
        var overview = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_overview", new { store_id = StoreV });
        var search = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreV });

        // Assert
        Assert.Equal((2, 1, 1, 0), GetFigures(Assert.Single(overview.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray())));
        Assert.Equal(2, search.EnsureSucceeded().Json.GetProperty("products").GetProperty("total_count").GetInt32());
        AssertNoLeak(overview);
        AssertNoLeak(search);
    }

    // A link into V's category that names another target catalog, as the API saves it: the storefront's outline follows the
    // category, so the store sells the linked catalog's products, and they stay out of B's lists and figures.
    [Fact]
    public async Task ManagerOfX_DoesNotSeeProductsLinkedUnderAnotherTargetCatalog()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        var part = AddCatalogW(data);
        data.CategoryLinks.Add(new CategoryLink { ListEntryId = part.CategoryId, ListEntryType = "Category", CatalogId = "another-catalog", CategoryId = OfficeV });
        // Its category is linked into X's root as well, so the index lists it under X.
        data.CategoryLinks.Add(new CategoryLink { ListEntryId = part.CategoryId, ListEntryType = "Category", CatalogId = CatalogX });
        await using var harness = CreateHarness(data);

        // Act
        var overview = await CallAsync(harness, ManagerOf(CatalogX), "get_catalog_overview", new { store_id = StoreV });
        var search = await CallAsync(harness, ManagerOf(CatalogX), "search_products", new { store_id = StoreV });

        // Assert
        var store = Assert.Single(overview.EnsureSucceeded().Json.GetProperty("stores").GetProperty("items").EnumerateArray());
        Assert.Equal((2, 1, 1, 0), GetFigures(store));
        Assert.Equal(["X-100", "X-200"], GetSkus(search.EnsureSucceeded().Json.GetProperty("products")));
        Assert.Equal(2, search.Json.GetProperty("products").GetProperty("total_count").GetInt32());
        AssertNoLeak(overview);
        AssertNoLeak(search);
    }

    // A part whose permission the user lacks: its findings are counted, not shown, and still keep the product from sale.
    [Fact]
    public async Task GetProduct_APartTheUserMayNotRead_IsCountedAndStillBlocksTheSale()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData(),
            services => services.AddSingleton<VirtoCommerce.CatalogModule.Core.MCP.ProductSetup.IProductSetupPart>(
                new ProductSetup.TestProductCheckPart("price", "price-missing") { Permissions = new() { Read = "pricing:read" } }));

        // Act
        var result = await CallAsync(harness, ManagerOf(CatalogX), "get_product", new { store_id = StoreX, sku = "X-100" });

        // Assert
        var json = result.EnsureSucceeded().Json;
        Assert.False(json.GetProperty("can_be_bought").GetBoolean());
        Assert.Empty(json.GetProperty("findings").EnumerateArray());
        Assert.Equal(["Test part price: 1 finding not shown, 1 blocking the sale (needs pricing:read)"],
            json.GetProperty("not_shown").EnumerateArray().Select(x => x.GetString()));
    }

    // No tool reads a catalog's products in full: the test index fails a request for more than a page, and the test
    // product search one that names no SKU, id, main product or category.
    [Fact]
    public async Task EveryReadTool_ReadsBoundedPagesOnly()
    {
        // Arrange
        await using var harness = CreateHarness(new CatalogToolsTestData());
        var user = GlobalManager();

        // Act
        var results = new[]
        {
            await CallAsync(harness, user, "get_catalog_overview"),
            await CallAsync(harness, user, "get_catalog_structure", new { catalog_id = CatalogX }),
            await CallAsync(harness, user, "get_virtual_catalog", new { catalog_id = CatalogV }),
            await CallAsync(harness, user, "search_products", new { store_id = StoreV, keyword = "printer" }),
            await CallAsync(harness, user, "get_product", new { store_id = StoreV, sku = "X-100" }),
            await CallAsync(harness, user, "find_products_not_for_sale", new { store_id = StoreV }),
            await CallAsync(harness, user, "find_products_not_for_sale", new { store_id = StoreV, skus = new[] { "X-100", "LEAK-Y-100" } }),
        };

        // Assert
        Assert.All(results, x => x.EnsureSucceeded());
        var index = harness.Services.GetRequiredService<InMemoryProductIndex>();
        Assert.All(index.Requests, x => Assert.True(x.Take <= 50 || x.Filter is IdsFilter, $"Take {x.Take}"));
    }

    // A catalog W that B may not read, marked as Y is, with one active product.
    private static CatalogProduct AddCatalogW(CatalogToolsTestData data)
    {
        data.Catalogs.Add(new Catalog { Id = "LEAK-Y-w", Name = "LEAK-Y W", Languages = [new CatalogLanguage { LanguageCode = "en-US", IsDefault = true }] });
        data.Categories.Add(new Category { Id = "LEAK-Y-w-parts", Name = "LEAK-Y Parts", CatalogId = "LEAK-Y-w", IsActive = true });
        var part = new CatalogProduct
        {
            Id = "LEAK-Y-w1", Code = "LEAK-Y-W-1", Name = "LEAK-Y part", CatalogId = "LEAK-Y-w", CategoryId = "LEAK-Y-w-parts",
            IsActive = true, IsBuyable = true, StartDate = Saved.AddDays(-30), ModifiedDate = Saved,
        };
        data.Products.Add(part);

        return part;
    }

    private static List<string> GetSkus(JsonElement page)
    {
        return page.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("sku").GetString()).Order(StringComparer.Ordinal).ToList();
    }

    private static (int InIndex, int Listed, int Hidden, int OutsideDates) GetFigures(JsonElement store)
    {
        var products = store.GetProperty("products");

        return (products.GetProperty("in_search_index").GetInt32(), products.GetProperty("listed").GetInt32(),
            products.GetProperty("hidden").GetInt32(), products.GetProperty("outside_dates").GetInt32());
    }

    // The answers leave out what is null.
    private static bool IsLeftOut(JsonElement element, string name)
    {
        return !element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null;
    }

    // No name, SKU or id of the catalog Y, in the answer or in the error.
    private static void AssertNoLeak(McpToolCallResult result)
    {
        Assert.DoesNotContain(Marker, result.Text ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Marker, result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
