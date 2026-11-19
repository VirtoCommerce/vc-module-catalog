using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.MCP.StoreSetup;

public class CatalogStoreSetupPartTests
{
    private const string CatalogId = "eu-catalog";

    // The catalogs saved so far, by id; the service returns copies, as the platform's does.
    private readonly Dictionary<string, Catalog> _saved = [];
    private readonly Mock<ICatalogService> _catalogService = new();

    public CatalogStoreSetupPartTests()
    {
        _catalogService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool _) => ids.Where(_saved.ContainsKey).Select(x => Copy(_saved[x])).ToList());
        _catalogService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()))
            .Callback((IList<Catalog> catalogs) => catalogs.ToList().ForEach(x => _saved[x.Id] = Copy(x)))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task PlanAsync_ANewStore_GetsANewCatalog_ThatThePlannedStorePointsAt()
    {
        //Arrange
        var context = CreateContext(new Store { Id = "eu-store", DefaultLanguage = "de-DE" }, isNewStore: true);

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Name = " EU catalog " });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.True(plan.IsNewCatalog);
        Assert.Equal("EU catalog", plan.Catalog.Name);
        Assert.Equal(plan.Catalog.Id, context.Store.Catalog);
        var language = Assert.Single(plan.Catalog.Languages);
        Assert.Equal(("de-DE", true), (language.LanguageCode, language.IsDefault));
        var change = Assert.Single(plan.Changes);
        Assert.Equal(("name", (object)null, (object)"EU catalog"), (change.Field, change.From, change.To));
        _catalogService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()), Times.Never);
    }

    [Fact]
    public async Task PlanAsync_AnExistingStore_RenamesItsCatalog_WithoutSavingIt()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "Old name" };

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Name = "EU catalog" });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.False(plan.IsNewCatalog);
        var change = Assert.Single(plan.Changes);
        Assert.Equal(("name", (object)"Old name", (object)"EU catalog"), (change.Field, change.From, change.To));
        Assert.Equal("Old name", _saved[CatalogId].Name);
    }

    // A field that is left out changes nothing, and neither does the name the catalog already has; a change of letter
    // case is a change.
    [Theory]
    [InlineData(null, 0)]
    [InlineData("EU catalog", 0)]
    [InlineData("EU Catalog", 1)]
    public async Task PlanAsync_ChangesOnlyAnotherName(string name, int expectedChanges)
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog" };

        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Name = name });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal(expectedChanges, plan.Changes.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null, 129)]
    public async Task PlanAsync_RefusesAnEmptyOrTooLongName(string name, int length = 0)
    {
        //Arrange
        var section = new CatalogStoreSetupSection { Name = name ?? new string('n', length) };

        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateStore()), section);

        //Assert
        Assert.Equal(["The catalog needs a name of 1 to 128 characters."], plan.Errors);
        Assert.Empty(plan.Changes);
    }

    // A create that failed after the store was saved leaves the store pointing at a catalog that is not there.
    [Fact]
    public async Task PlanAsync_AStoreWhoseCatalogIsMissing_GetsItUnderItsCatalogId()
    {
        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Name = "EU catalog" });

        //Assert
        Assert.True(plan.IsNewCatalog);
        Assert.Equal(CatalogId, plan.Catalog.Id);
    }

    [Fact]
    public async Task ApplyAsync_SavesTheNewCatalog_AndARepeatChangesNothing()
    {
        //Arrange
        var part = CreatePart();
        var context = CreateContext(new Store { Id = "eu-store", DefaultLanguage = "de-DE" }, isNewStore: true);
        var section = new CatalogStoreSetupSection { Name = "EU catalog" };

        //Act
        await part.ApplyAsync(context, await part.PlanAsync(context, section));
        var repeat = await part.PlanAsync(context, section);

        //Assert
        var catalog = Assert.Single(_saved.Values);
        Assert.Equal((context.Store.Catalog, "EU catalog"), (catalog.Id, catalog.Name));
        Assert.Empty(repeat.Changes);
    }

    // Applying reads the catalog again: what was changed in the back office since the plan stays.
    [Fact]
    public async Task ApplyAsync_RenamesTheCatalogReadAgain()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "Old name" };
        var part = CreatePart();
        var context = CreateContext(CreateStore());
        var plan = await part.PlanAsync(context, new CatalogStoreSetupSection { Name = "EU catalog" });
        _saved[CatalogId].IsVirtual = true;

        //Act
        await part.ApplyAsync(context, plan);

        //Assert
        Assert.Equal(("EU catalog", true), (_saved[CatalogId].Name, _saved[CatalogId].IsVirtual));
    }

    [Fact]
    public async Task CheckAsync_FindsAMissingCatalog_ButNotOneThePlanCreates()
    {
        //Arrange
        var part = CreatePart();
        var missing = CreateContext(CreateStore());
        var planned = CreateContext(new Store { Id = "eu-store" }, isNewStore: true);
        planned.Plans[CatalogStoreSetupPart.PartName] = await part.PlanAsync(planned, new CatalogStoreSetupSection { Name = "EU catalog" });

        //Act
        var missingFindings = await part.CheckAsync(missing);
        var plannedFindings = await part.CheckAsync(planned);

        //Assert
        var finding = Assert.Single(missingFindings);
        Assert.Equal(("catalog-not-found", McpFindingSeverities.Critical, CatalogStoreSetupPart.PartName), (finding.Id, finding.Severity, finding.Category));
        Assert.Contains(CatalogId, finding.Evidence);
        Assert.Empty(plannedFindings);
    }

    [Fact]
    public async Task GetStateAsync_IsTheCatalogsName()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog" };

        //Act
        var state = (CatalogStoreSetupSection)await CreatePart().GetStateAsync(CreateStore());

        //Assert
        Assert.Equal("EU catalog", state.Name);
    }

    [Fact]
    public void Permissions_AreCatalogsOwn_GrantedGlobally()
    {
        //Act
        var permissions = CreatePart().Permissions;

        //Assert
        Assert.Equal(("catalog:read", "catalog:update", "catalog:create", (string)null), (permissions.Read, permissions.Update, permissions.Create, permissions.ScopeType));
    }

    private CatalogStoreSetupPart CreatePart()
    {
        return new CatalogStoreSetupPart(_catalogService.Object);
    }

    private static Store CreateStore()
    {
        return new Store { Id = "eu-store", Catalog = CatalogId, DefaultLanguage = "de-DE" };
    }

    private static StoreSetupContext CreateContext(Store store, bool isNewStore = false)
    {
        return new StoreSetupContext { Store = store, IsNewStore = isNewStore };
    }

    private static Catalog Copy(Catalog catalog)
    {
        return new Catalog
        {
            Id = catalog.Id,
            Name = catalog.Name,
            IsVirtual = catalog.IsVirtual,
            Languages = catalog.Languages?.Select(x => new CatalogLanguage { LanguageCode = x.LanguageCode, IsDefault = x.IsDefault }).ToList(),
        };
    }
}
