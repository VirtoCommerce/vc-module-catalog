using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;
using static VirtoCommerce.CatalogModule.Tests.MCP.CatalogToolsTestData;

namespace VirtoCommerce.CatalogModule.Tests.MCP.ProductSetup;

[Trait("Category", "Unit")]
public class ProductSetupServiceTests
{
    [Fact]
    public async Task GetParts_CatalogsPartFirst_ThenByName_TheLastOfANameWins()
    {
        // Arrange
        var replacement = new TestProductCheckPart("price", "replaced");
        await using var provider = CreateServices(new CatalogToolsTestData(), services =>
        {
            services.AddSingleton<IProductSetupPart>(new TestProductCheckPart("stock"));
            services.AddSingleton<IProductSetupPart>(new TestProductCheckPart("price"));
            services.AddSingleton<IProductSetupPart>(replacement);
        });

        // Act
        var parts = provider.GetRequiredService<IProductSetupService>().GetParts();

        // Assert
        Assert.Equal(["product", "price", "stock"], parts.Select(x => x.Name));
        Assert.Same(replacement, parts[1]);
    }

    [Fact]
    public async Task CheckAsync_AFindingBelongsToThePartThatFoundIt()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        await using var provider = CreateServices(data, services => services.AddSingleton<IProductSetupPart>(new TestProductCheckPart("price", "price-missing")));

        // Act
        var findings = await provider.GetRequiredService<IProductSetupService>().CheckAsync(data.Stores.First(x => x.Id == StoreX), [LaserPrinter]);

        // Assert
        var finding = Assert.Single(findings);
        Assert.Equal(("price-missing", "price"), (finding.Id, finding.Category));
    }

    [Fact]
    public async Task CheckAsync_APartThatFails_GivesOneCriticalFindingPerProduct_WithoutItsMessage()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        await using var provider = CreateServices(data, services => services.AddSingleton<IProductSetupPart>(new TestProductCheckPart("stock", fails: true)));

        // Act
        var findings = await provider.GetRequiredService<IProductSetupService>().CheckAsync(data.Stores.First(x => x.Id == StoreV), [LaserPrinter, Drill]);

        // Assert
        Assert.Equal([(LaserPrinter, "stock-not-checked"), (Drill, "stock-not-checked")], findings.Select(x => (x.ObjectId, x.Id)));
        Assert.All(findings, x => Assert.Equal(McpFindingSeverities.Critical, x.Severity));
        Assert.All(findings, x => Assert.DoesNotContain("own message", $"{x.Title} {x.Evidence} {x.Fix}"));
    }

    [Fact]
    public async Task CheckAsync_AnIndexThatCannotBeRead_LeavesTheProductPartNotChecked()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        await using var provider = CreateServices(data, services => services.AddSingleton<ISearchProvider>(new FailingSearchProvider()));

        // Act
        var finding = Assert.Single(await provider.GetRequiredService<IProductSetupService>().CheckAsync(data.Stores.First(x => x.Id == StoreX), [LaserPrinter]));

        // Assert
        Assert.Equal(("product-not-checked", McpFindingSeverities.Critical), (finding.Id, finding.Severity));
    }

    [Fact]
    public async Task GetStoreCatalogsAsync_AVirtualStore_SellsFromThePhysicalCatalogsItLinks()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        await using var provider = CreateServices(data);

        // Act
        var catalogs = await provider.GetRequiredService<IProductSetupService>().GetStoreCatalogsAsync(data.Stores.First(x => x.Id == StoreV));

        // Assert
        Assert.Equal(CatalogV, catalogs.StoreCatalog.Id);
        Assert.Equal([CatalogY, CatalogX], catalogs.Catalogs.Select(x => x.Id));
    }

    [Fact]
    public async Task GetStoreCatalogsAsync_FollowsLinksIntoTheLinkedSubtrees()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        // The physical catalog Z links a category into X's Laser, under the category V links.
        data.Catalogs.Add(new Catalog { Id = "catalog-z", Name = "Z", Languages = [new CatalogLanguage { LanguageCode = "en-US", IsDefault = true }] });
        data.Categories.Add(new Category { Id = "z-toner", Name = "Toner", CatalogId = "catalog-z", IsActive = true });
        data.CategoryLinks.Add(new CategoryLink { ListEntryId = "z-toner", ListEntryType = nameof(Category), CatalogId = CatalogX, CategoryId = LaserX });
        await using var provider = CreateServices(data);

        // Act
        var catalogs = await provider.GetRequiredService<IProductSetupService>().GetStoreCatalogsAsync(data.Stores.First(x => x.Id == StoreV));

        // Assert
        Assert.Contains("catalog-z", catalogs.Catalogs.Select(x => x.Id));
        Assert.Equal(["catalog-z"], catalogs.LinkedFrom[CatalogX]);
    }

    [Fact]
    public async Task GetStoreCatalogsAsync_ALinkThatClosesALoop_EndsTheWalk()
    {
        // Arrange
        var data = new CatalogToolsTestData();
        // Printers linked into its own subcategory, as the API allows.
        data.CategoryLinks.Add(new CategoryLink { ListEntryId = PrintersX, ListEntryType = nameof(Category), CatalogId = CatalogX, CategoryId = LaserX });
        await using var provider = CreateServices(data);

        // Act
        var catalogs = await provider.GetRequiredService<IProductSetupService>().GetStoreCatalogsAsync(data.Stores.First(x => x.Id == StoreV));

        // Assert
        Assert.Equal([CatalogY, CatalogX], catalogs.Catalogs.Select(x => x.Id));
        Assert.Empty(catalogs.LinkedFrom[CatalogX]);
    }

    private static ServiceProvider CreateServices(CatalogToolsTestData data, Action<IServiceCollection> configure = null)
    {
        var services = new ServiceCollection();
        CatalogToolsTestContext.AddCatalogServices(services, data);
        configure?.Invoke(services);

        return services.BuildServiceProvider();
    }

    private sealed class FailingSearchProvider : ISearchProvider
    {
        public Task<SearchResponse> SearchAsync(string documentType, SearchRequest request) => throw new InvalidOperationException("The index is down.");

        public Task DeleteIndexAsync(string documentType) => throw new NotSupportedException();

        public Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> documents) => throw new NotSupportedException();

        public Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents) => throw new NotSupportedException();
    }
}
