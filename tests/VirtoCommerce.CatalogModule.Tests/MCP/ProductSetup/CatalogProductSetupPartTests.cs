using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;
using VirtoCommerce.MCP.Core.Models;
using Xunit;
using static VirtoCommerce.CatalogModule.Tests.MCP.CatalogToolsTestData;

namespace VirtoCommerce.CatalogModule.Tests.MCP.ProductSetup;

// The rules of the product check, one test each, through the setup service over the test data and its search index.
[Trait("Category", "Unit")]
public class CatalogProductSetupPartTests
{
    [Fact]
    public async Task CheckAsync_AProductReadyForSale_HasNoFinding()
    {
        // Act
        var findings = await CheckAsync(new CatalogToolsTestData(), StoreV, LaserPrinter);

        // Assert
        Assert.Empty(findings);
    }

    [Fact]
    public async Task CheckAsync_AProductMissingFromTheIndex_CannotBeBought()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.NotIndexed.Add(LaserPrinter);

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.NotInIndex, McpFindingSeverities.Critical), (finding.Id, finding.Severity));
        Assert.Equal("product", finding.Category);
        Assert.Equal(LaserPrinter, finding.ObjectId);
    }

    [Fact]
    public async Task CheckAsync_AnIndexOlderThanTheLastSave_IsAWarning()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.IndexedModifiedDates[LaserPrinter] = Saved.AddMinutes(-5);

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.IndexOutdated, McpFindingSeverities.Warning), (finding.Id, finding.Severity));
        Assert.Equal("Saved 2026-11-01 10:00:00; the index holds the version of 2026-11-01 09:55:00 (UTC).", finding.Evidence);
    }

    [Fact]
    public async Task CheckAsync_AnIndexWithinTheTolerance_IsUpToDate()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.IndexedModifiedDates[LaserPrinter] = Saved.AddMilliseconds(-500);

        // Act
        var findings = await CheckAsync(data, StoreX, LaserPrinter);

        // Assert
        Assert.Empty(findings);
    }

    [Theory]
    [InlineData(1, null)]
    [InlineData(-10, -1)]
    public async Task CheckAsync_AProductOutsideItsDates_CannotBeBought(int startDays, int? endDays)
    {
        // Arrange
        var data = new CatalogToolsTestData();
        var product = data.GetProduct(LaserPrinter);
        product.StartDate = DateTime.UtcNow.AddDays(startDays);
        product.EndDate = endDays == null ? null : DateTime.UtcNow.AddDays(endDays.Value);

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.OutsideDates, McpFindingSeverities.Critical), (finding.Id, finding.Severity));
    }

    [Fact]
    public async Task CheckAsync_AnInactiveProduct_CannotBeBought()
    {
        // Act
        var finding = Assert.Single(await CheckAsync(new CatalogToolsTestData(), StoreX, OldPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.Inactive, McpFindingSeverities.Critical), (finding.Id, finding.Severity));
    }

    [Fact]
    public async Task CheckAsync_AProductUnderAnInactiveCategory_CannotBeBought()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.GetCategory(PrintersX).IsActive = false;

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.CategoryInactive, McpFindingSeverities.Critical), (finding.Id, finding.Severity));
        Assert.Equal("The category 'Printers' is inactive.", finding.Evidence);
    }

    [Fact]
    public async Task CheckAsync_AProductThatIsNotBuyable_CannotBeBought()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.GetProduct(LaserPrinter).IsBuyable = false;

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.NotBuyable, McpFindingSeverities.Critical), (finding.Id, finding.Severity));
    }

    [Fact]
    public async Task CheckAsync_AProductTheStoresCatalogDoesNotReach_CannotBeBought()
    {
        // Act
        var finding = Assert.Single(await CheckAsync(new CatalogToolsTestData(), StoreY, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.NotInStoreCatalog, McpFindingSeverities.Critical), (finding.Id, finding.Severity));
        Assert.Equal("No category of the product reaches the store's catalog 'LEAK-Y Catalog'; the product is in 'Printers > Laser'.", finding.Evidence);
    }

    [Fact]
    public async Task CheckAsync_AProductWithVariations_SaysCustomersBuyThem()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.Products.Add(new CatalogProduct
        {
            Id = "x-p1-red", Code = "X-100-RED", Name = "Laser printer, red", CatalogId = CatalogX, CategoryId = LaserX, MainProductId = LaserPrinter,
            IsActive = true, IsBuyable = true, StartDate = Saved.AddDays(-30), ModifiedDate = Saved,
        });

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.HasVariations, McpFindingSeverities.Info, "1 variation."), (finding.Id, finding.Severity, finding.Evidence));
    }

    [Fact]
    public async Task CheckAsync_AProductWithoutImages_IsAWarning()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.GetProduct(LaserPrinter).Images = [];

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.NoImage, McpFindingSeverities.Warning), (finding.Id, finding.Severity));
    }

    [Fact]
    public async Task CheckAsync_AStoreLanguageWithoutADescription_IsAWarning()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.Stores.First(x => x.Id == StoreX).Languages = ["en-US", "de-DE"];

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.NoDescription, McpFindingSeverities.Warning), (finding.Id, finding.Severity));
        Assert.Equal("No description in: de-DE.", finding.Evidence);
    }

    [Fact]
    public async Task CheckAsync_ADescriptionWithoutALanguage_ServesEveryLanguage()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.Stores.First(x => x.Id == StoreX).Languages = ["en-US", "de-DE"];
        data.GetProduct(LaserPrinter).Reviews = [new EditorialReview { Content = "For every language." }];

        // Act
        var findings = await CheckAsync(data, StoreX, LaserPrinter);

        // Assert
        Assert.Empty(findings);
    }

    [Fact]
    public async Task CheckAsync_ARequiredPropertyWithoutAValue_IsAWarning()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.GetProduct(LaserPrinter).Properties =
        [
            new Property { Name = "color", Required = true, Type = PropertyType.Product, Values = [] },
            new Property { Name = "pages_per_minute", Required = true, Type = PropertyType.Product, Values = [new PropertyValue { Value = 40 }] },
            new Property { Name = "size", Required = true, Type = PropertyType.Variation, Values = [] },
        ];

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, LaserPrinter));

        // Assert
        Assert.Equal((CatalogProductSetupPart.FindingIds.RequiredPropertyMissing, McpFindingSeverities.Warning), (finding.Id, finding.Severity));
        Assert.Equal("No value: color.", finding.Evidence);
    }

    [Theory]
    [InlineData(true, true, "The storefront shows it after the search index updates, usually within a minute.")]
    [InlineData(false, true, "Indexing on change is switched off: the scheduled indexing job adds the change (schedule: every 5 minutes).")]
    [InlineData(false, false, "Indexing is switched off: build the index on the Search index screen.")]
    public async Task CheckAsync_AProductMissingFromTheIndex_SaysHowTheIndexUpdates(bool eventIndexing, bool scheduledIndexing, string sentence)
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.NotIndexed.Add(LaserPrinter);

        // Act
        var finding = Assert.Single(await CheckAsync(data, StoreX, [LaserPrinter], eventIndexing, scheduledIndexing));

        // Assert
        Assert.StartsWith(sentence, finding.Fix);
    }

    [Fact]
    public async Task GetStatesAsync_GivesTheProductInTheShapeOfARow()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        data.Stores.First(x => x.Id == StoreV).Languages = ["en-US", "de-DE"];
        var product = data.GetProduct(LaserPrinter);
        product.Reviews = [.. product.Reviews, new EditorialReview { Content = "Über den Drucker.", LanguageCode = "de-DE", ReviewType = "QuickReview" }];
        product.Properties = [new Property { Name = "features", Multivalue = true, Type = PropertyType.Product, Values = [new PropertyValue { Value = "Duplex" }, new PropertyValue { Value = "Wi-Fi" }] }];

        // Act
        var states = await GetStatesAsync(data, StoreV, LaserPrinter);

        // Assert
        var state = Assert.IsType<CatalogProductSection>(states["product"][LaserPrinter]);
        Assert.Equal(("X-100", "Laser printer", "Office > Printers > Laser"), (state.Sku, state.Name, state.Category));
        Assert.Equal("About Laser printer.", state.Description);
        Assert.Equal("Über den Drucker.", state.Descriptions["de-DE"]);
        Assert.Equal(["Duplex", "Wi-Fi"], Assert.IsType<List<object>>(state.Properties["features"]));
        Assert.Equal((true, true, true), (state.Active, state.Buyable, state.TrackInventory));
    }

    public static Task<IList<McpFinding>> CheckAsync(CatalogToolsTestData data, string storeId, params string[] productIds)
    {
        return CheckAsync(data, storeId, productIds, eventIndexing: true, scheduledIndexing: true);
    }

    public static async Task<IList<McpFinding>> CheckAsync(CatalogToolsTestData data, string storeId, string[] productIds, bool eventIndexing, bool scheduledIndexing)
    {
        await using var provider = CreateServices(data, eventIndexing, scheduledIndexing);

        return await provider.GetRequiredService<IProductSetupService>().CheckAsync(data.Stores.First(x => x.Id == storeId), productIds);
    }

    private static async Task<IDictionary<string, IDictionary<string, object>>> GetStatesAsync(CatalogToolsTestData data, string storeId, params string[] productIds)
    {
        await using var provider = CreateServices(data, eventIndexing: true, scheduledIndexing: true);

        return await provider.GetRequiredService<IProductSetupService>().GetStatesAsync(data.Stores.First(x => x.Id == storeId), productIds);
    }

    private static ServiceProvider CreateServices(CatalogToolsTestData data, bool eventIndexing, bool scheduledIndexing)
    {
        var services = new ServiceCollection();
        CatalogToolsTestContext.AddCatalogServices(services, data, eventIndexing, scheduledIndexing);

        return services.BuildServiceProvider();
    }
}
