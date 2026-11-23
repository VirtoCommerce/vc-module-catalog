using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.MCP.StoreSetup;

[Trait("Category", "Unit")]
public class CatalogStoreSetupPartTests
{
    private const string CatalogId = "eu-catalog";
    private const string MainId = "main";

    // The catalogs saved so far, by id, and the stores; the services return copies, as the platform's do, or the saved
    // catalogs themselves when asked for no copy, as the platform returns its cached ones.
    private readonly Dictionary<string, Catalog> _saved = [];
    private readonly List<Store> _stores = [];
    private readonly Mock<ICatalogService> _catalogService = new();

    public CatalogStoreSetupPartTests()
    {
        _catalogService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((IList<string> ids, string _, bool clone) => ids.Where(_saved.ContainsKey).Select(x => clone ? Copy(_saved[x]) : _saved[x]).ToList());
        _catalogService
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()))
            .Callback((IList<Catalog> catalogs) => catalogs.ToList().ForEach(x =>
            {
                // As CatalogEntity.FromModel: a catalog is saved with a default language.
                if (x.DefaultLanguage == null)
                {
                    throw new InvalidOperationException($"Catalog '{x.Id}' has no default language.");
                }

                _saved[x.Id] = Copy(x);
            }))
            .Returns(Task.CompletedTask);
    }

    // A new store by name: a new, empty catalog with every language of the store, and the row says so.
    [Fact]
    public async Task PlanAsync_ANewStore_ByName_GetsANewEmptyCatalog_WithEveryLanguageOfTheStore()
    {
        //Arrange
        var context = CreateContext(new Store { Id = "eu-store", DefaultLanguage = "de-DE", Languages = ["en-US", "de-DE", "fr-FR"] }, isNewStore: true);

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Name = " EU catalog " });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Empty(plan.Warnings);
        Assert.True(plan.IsNewCatalog);
        Assert.Equal("EU catalog", plan.Catalog.Name);
        Assert.Equal(plan.Catalog.Id, context.Store.Catalog);
        Assert.Equal([("de-DE", true), ("en-US", false), ("fr-FR", false)], plan.Catalog.Languages.Select(x => (x.LanguageCode, x.IsDefault)));
        var change = Assert.Single(plan.Changes);
        Assert.Equal((CatalogStoreSetupPart.StoreCatalogField, (object)null, (object)"a new, empty catalog 'EU catalog'"), (change.Field, change.From, change.To));
        // It creates a catalog, and reads the catalogs for one with the same name.
        Assert.Equal(["catalog:create", "catalog:read"], plan.RequiredPermissions);
        _catalogService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()), Times.Never);
    }

    // Catalog names are not unique: the preview warns with the id of the catalog that has the name.
    [Fact]
    public async Task PlanAsync_ANewStore_ByANameACatalogHas_IsWarned_WithItsId()
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main" };

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(CreateNewStore(), isNewStore: true), new CatalogStoreSetupSection { Name = "main" });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.True(plan.IsNewCatalog);
        Assert.Equal("A catalog named 'main' exists already (main). To sell from it instead of a new, empty catalog, send its id instead of the name.", Assert.Single(plan.Warnings));
    }

    // A new store by id, alone or with the catalog's own name, as a state copied from another store sends it.
    [Theory]
    [InlineData(null)]
    [InlineData("Main")]
    public async Task PlanAsync_ANewStore_ById_SellsFromTheExistingCatalog_SharedWithTheOtherStores(string name)
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        _stores.AddRange([new Store { Id = "de-store", Catalog = MainId }, new Store { Id = "at-store", Catalog = MainId }, new Store { Id = "us-store", Catalog = "us" }]);
        var context = CreateContext(CreateNewStore(), isNewStore: true);

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Id = MainId, Name = name });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.False(plan.IsNewCatalog);
        Assert.Equal(MainId, context.Store.Catalog);
        var change = Assert.Single(plan.Changes);
        Assert.Equal((CatalogStoreSetupPart.StoreCatalogField, "the existing catalog 'Main' (main), shared with 2 other stores"), (change.Field, change.To));
        // A catalog it reuses as it is: shown, not changed.
        Assert.Equal(["catalog:read"], plan.RequiredPermissions);
        _catalogService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()), Times.Never);
    }

    [Fact]
    public async Task PlanAsync_ANewStore_ByAnIdThatDoesNotExist_IsRefused()
    {
        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateNewStore(), isNewStore: true), new CatalogStoreSetupSection { Id = "missing" });

        //Assert
        Assert.Equal(["Catalog 'missing' does not exist. get_store_setup_options lists the catalogs."], plan.Errors);
        // Whether a catalog exists is the catalog's data.
        Assert.Equal(["catalog:read"], plan.RequiredPermissions);
    }

    // Creating a store never renames a catalog: the refusal says which field to leave out for each intent.
    [Fact]
    public async Task PlanAsync_ANewStore_ByIdWithAnotherName_IsRefused_AndSaysWhichFieldToLeaveOut()
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main" };
        var context = CreateContext(CreateNewStore(), isNewStore: true);

        //Act
        var plan = await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Id = MainId, Name = "EU catalog" });

        //Assert
        Assert.Equal(["Creating a store never renames a catalog. To sell from the catalog 'Main' (main), leave out name; to create a new catalog named 'EU catalog', leave out id."], plan.Errors);
        Assert.Null(context.Store.Catalog);
    }

    [Fact]
    public async Task PlanAsync_ANewStore_WithNeitherIdNorName_IsRefused()
    {
        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateNewStore(), isNewStore: true), new CatalogStoreSetupSection { Id = " " });

        //Assert
        Assert.Equal(["A new store needs its catalog: send name to create a new, empty catalog, or id to sell from an existing one."], plan.Errors);
        Assert.Empty(plan.RequiredPermissions);
    }

    // An existing store's name renames its catalog, alone or with the store's own catalog id; other stores using the
    // catalog are counted in a warning.
    [Theory]
    [InlineData(null)]
    [InlineData("EU-CATALOG")]
    public async Task PlanAsync_AnExistingStore_RenamesItsCatalog_AndWarnsWhenOtherStoresUseIt(string id)
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "Old name", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        _stores.AddRange([CreateStore(), new Store { Id = "at-store", Catalog = CatalogId }]);

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Id = id, Name = "New name" });

        //Assert
        Assert.Empty(plan.Errors);
        var change = Assert.Single(plan.Changes);
        Assert.Equal(("name", (object)"Old name", (object)"New name"), (change.Field, change.From, change.To));
        Assert.Equal("1 other store sells from this catalog and shows the new name.", Assert.Single(plan.Warnings));
        Assert.Equal(["catalog:read", "catalog:update"], plan.RequiredPermissions);
        Assert.Equal("Old name", _saved[CatalogId].Name);
    }

    // Changing a store's catalog is done in the back office: the refusal names the screen.
    [Fact]
    public async Task PlanAsync_AnExistingStore_ByAnotherId_IsRefused_AndNamesTheScreen()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog" };
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main" };

        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Id = MainId });

        //Assert
        Assert.Equal(["Store 'EU store' sells from the catalog 'EU catalog' (eu-catalog). A store's catalog is changed in the back office: Stores > EU store, field Catalog."], plan.Errors);
    }

    // The catalogs table holds names of up to 128 characters.
    [Theory]
    [InlineData(0, false)]
    [InlineData(129, false)]
    [InlineData(128, true)]
    public async Task PlanAsync_TakesANameOf1To128Characters(int length, bool accepted)
    {
        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateNewStore(), isNewStore: true), new CatalogStoreSetupSection { Name = new string('n', length) });

        //Assert
        Assert.Equal(accepted ? [] : ["The catalog needs a name of 1 to 128 characters."], plan.Errors);
    }

    [Fact]
    public async Task PlanAsync_RefusesANameOfSpacesOnly()
    {
        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Name = "   " });

        //Assert
        Assert.Equal(["The catalog needs a name of 1 to 128 characters."], plan.Errors);
        Assert.Empty(plan.RequiredPermissions);
    }

    // A store whose catalog is missing gets it again under its catalog id; its id alone names nothing.
    [Fact]
    public async Task PlanAsync_AStoreWhoseCatalogIsMissing_GetsItAgainUnderItsCatalogId_ByName_NotById()
    {
        //Arrange
        var context = CreateContext(CreateStore());

        //Act
        var byName = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Name = "EU catalog" });
        var byId = await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Id = CatalogId });

        //Assert
        Assert.Empty(byName.Errors);
        Assert.True(byName.IsNewCatalog);
        Assert.Equal(CatalogId, byName.Catalog.Id);
        Assert.Equal("a new, empty catalog 'EU catalog', under the store's catalog id 'eu-catalog'", Assert.Single(byName.Changes).To);
        Assert.Equal(["catalog:read", "catalog:create"], byName.RequiredPermissions);
        Assert.Equal(["Catalog 'eu-catalog' does not exist. Send name to create it again under this id, or give the store an existing catalog in the back office: Stores > EU store, field Catalog."], byId.Errors);
        Assert.Equal(["catalog:read"], byId.RequiredPermissions);
    }

    // A store saved without a catalog id gets its catalog in the back office: a catalog the section created would never
    // become the store's. A name, an id and an empty section are refused alike, before a catalog is read, so the
    // refusal needs no permission.
    [Theory]
    [InlineData(null, "EU catalog")]
    [InlineData(MainId, null)]
    [InlineData(null, null)]
    public async Task PlanAsync_AnExistingStoreWithoutACatalogId_IsRefused_BeforeAnyCatalogIsRead(string id, string name)
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        var store = CreateStore();
        store.Catalog = "";
        var context = CreateContext(store);

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Id = id, Name = name });

        //Assert
        Assert.Equal(["Store 'EU store' has no catalog. A store's catalog is set in the back office: Stores > EU store, field Catalog."], plan.Errors);
        Assert.Empty(plan.Changes);
        Assert.Null(plan.Catalog);
        Assert.False(plan.IsNewCatalog);
        Assert.Equal("", context.Store.Catalog);
        Assert.Empty(plan.RequiredPermissions);
    }

    // The catalog gains the store's languages it lacks, as a row, and keeps the ones the store does not have.
    [Fact]
    public async Task PlanAsync_TheCatalogGainsTheStoresLanguages_AndLosesNone()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }, new CatalogLanguage { LanguageCode = "it-IT" }] };
        var store = CreateStore();
        store.Languages = ["de-DE", "en-US"];

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(store), new CatalogStoreSetupSection { Id = CatalogId });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Equal(["en-US"], plan.AddedLanguages);
        var change = Assert.Single(plan.Changes);
        Assert.Equal("languages", change.Field);
        Assert.Equal(["de-DE", "it-IT"], (IEnumerable<string>)change.From);
        Assert.Equal(["de-DE", "it-IT", "en-US"], (IEnumerable<string>)change.To);
    }

    // A catalog a new store reuses gains the store's languages too.
    [Fact]
    public async Task PlanAsync_AReusedCatalog_GainsTheNewStoresLanguages()
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        var store = CreateNewStore();
        store.Languages = ["de-DE", "fr-FR"];

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(store, isNewStore: true), new CatalogStoreSetupSection { Id = MainId });

        //Assert
        Assert.Equal([CatalogStoreSetupPart.StoreCatalogField, "languages"], plan.Changes.Select(x => x.Field));
        Assert.Equal(["fr-FR"], plan.AddedLanguages);
        // A new store's languages change the catalog it reuses: catalog:update, not catalog:create.
        Assert.Equal(["catalog:read", "catalog:update"], plan.RequiredPermissions);
    }

    // A virtual catalog's texts live in the catalogs behind it: it gains no language, and none is missing.
    [Fact]
    public async Task PlanAsync_And_CheckAsync_SkipTheLanguagesOfAVirtualCatalog()
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main", IsVirtual = true, Languages = [] };
        var store = CreateNewStore();
        store.Languages = ["de-DE", "fr-FR"];
        var context = CreateContext(store, isNewStore: true);

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Id = MainId });
        var findings = await CreatePart().CheckAsync(context);

        //Assert
        Assert.Equal("the existing virtual catalog 'Main' (main), used by no other store", Assert.Single(plan.Changes).To);
        Assert.Empty(plan.AddedLanguages);
        Assert.Empty(findings);
    }

    // A section that changes nothing has no row, so the tool answers "Nothing to change".
    [Fact]
    public async Task PlanAsync_ASectionThatChangesNothing_HasNoChanges()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };

        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Id = CatalogId, Name = "EU catalog" });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Empty(plan.Changes);
        Assert.Equal(["catalog:read"], plan.RequiredPermissions);
    }

    // A name that differs from the catalog's only by spaces around it, as a catalog saved by hand may have, renames nothing:
    // its state sent back changes nothing.
    [Fact]
    public async Task PlanAsync_ANameThatDiffersOnlyBySpacesAroundIt_RenamesNothing()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = " EU catalog ", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };

        //Act
        var plan = await CreatePart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Name = "EU catalog" });

        //Assert
        Assert.Empty(plan.Errors);
        Assert.Empty(plan.Changes);
    }

    // The part reads copies: a plan leaves the cached catalog as it is.
    [Fact]
    public async Task PlanAsync_LeavesTheCachedCatalogAsItIs()
    {
        //Arrange
        var cached = new Catalog { Id = CatalogId, Name = "EU catalog", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        _saved[CatalogId] = cached;
        var store = CreateStore();
        store.Languages = ["de-DE", "en-US"];

        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(store), new CatalogStoreSetupSection { Name = "Renamed" });

        //Assert
        Assert.Equal("Renamed", plan.Catalog.Name);
        Assert.Same(cached, _saved[CatalogId]);
        Assert.Equal("EU catalog", cached.Name);
        Assert.Equal(["de-DE"], cached.Languages.Select(x => x.LanguageCode));
    }

    // A store's language its catalog lacks is a warning, with the fix to send; once the request plans the catalog
    // section, the catalog as planned has it.
    [Fact]
    public async Task CheckAsync_AStoreLanguageTheCatalogLacks_IsAWarning_UnlessTheRequestPlansTheCatalog()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        var store = CreateStore();
        store.Languages = ["de-DE", "en-US"];
        var withoutSection = CreateContext(store);
        var withSection = CreateContext(store);
        withSection.Plans[CatalogStoreSetupPart.PartName] = await CreatePart().PlanAsync(withSection, new CatalogStoreSetupSection { Id = CatalogId });

        //Act
        var findings = await CreatePart().CheckAsync(withoutSection);
        var findingsWithSection = await CreatePart().CheckAsync(withSection);

        //Assert
        var finding = Assert.Single(findings);
        Assert.Equal(("catalog-lacks-store-languages", McpFindingSeverities.Warning), (finding.Id, finding.Severity));
        Assert.Equal("Missing in the catalog 'EU catalog': en-US.", finding.Evidence);
        Assert.Equal("Send the catalog section with the store's catalog id (eu-catalog): the catalog gains the store's languages. " +
            "A person adds them in the back office too: Catalog > EU catalog, Languages.", finding.Fix);
        Assert.Null(finding.Screen);
        Assert.Empty(findingsWithSection);
    }

    [Fact]
    public async Task CheckAsync_FindsAMissingCatalog_ButNotOneThePlanCreates()
    {
        //Arrange
        var context = CreateContext(CreateNewStore(), isNewStore: true);
        var withoutPlan = await CreatePart().CheckAsync(context);
        context.Plans[CatalogStoreSetupPart.PartName] = await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Name = "EU catalog" });

        //Act
        var withPlan = await CreatePart().CheckAsync(context);

        //Assert
        var finding = Assert.Single(withoutPlan);
        Assert.Equal(("catalog-not-found", McpFindingSeverities.Critical, "Catalog: none."), (finding.Id, finding.Severity, finding.Evidence));
        // A new store gets its catalog from the section, so the fix names the section, not the back office.
        Assert.Equal("Send the catalog section with a name for a new, empty catalog, or with the id of an existing one.", finding.Fix);
        Assert.False(finding.ManualOnly);
        Assert.Empty(withPlan);
    }

    // The section refuses a store without a catalog id, so the fix names the back office only, and no tool.
    [Fact]
    public async Task CheckAsync_AStoreWithoutACatalogId_IsFixedInTheBackOfficeOnly()
    {
        //Arrange
        var store = CreateStore();
        store.Catalog = "";

        //Act
        var findings = await CreatePart().CheckAsync(CreateContext(store));

        //Assert
        var finding = Assert.Single(findings);
        Assert.Equal(("catalog-not-found", McpFindingSeverities.Critical, "Catalog: none."), (finding.Id, finding.Severity, finding.Evidence));
        Assert.Equal("A person gives the store an existing catalog in the back office: Stores > EU store, field Catalog.", finding.Fix);
        Assert.True(finding.ManualOnly);
    }

    // A store whose catalog was deleted: the evidence names its catalog id, and the fix the section that creates the
    // catalog again under it.
    [Fact]
    public async Task CheckAsync_AStoreWhoseCatalogIsMissing_NamesItsCatalogId_AndTheSectionThatCreatesItAgain()
    {
        //Act
        var findings = await CreatePart().CheckAsync(CreateContext(CreateStore()));

        //Assert
        var finding = Assert.Single(findings);
        Assert.Equal(("catalog-not-found", McpFindingSeverities.Critical, "Catalog 'eu-catalog': not found."), (finding.Id, finding.Severity, finding.Evidence));
        Assert.Equal("Send the catalog section with a name to create the catalog again under the store's catalog id, or give " +
            "the store an existing catalog in the back office.", finding.Fix);
        Assert.False(finding.ManualOnly);
    }

    [Fact]
    public async Task ApplyAsync_SavesTheNewCatalog_AndARepeatChangesNothing()
    {
        //Arrange
        var store = CreateStore();
        store.Catalog = null;
        var context = CreateContext(store, isNewStore: true);
        var plan = await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Name = "EU catalog" });

        //Act
        await CreatePart().ApplyAsync(context, plan);
        var repeat = await CreatePart().PlanAsync(CreateContext(context.Store), new CatalogStoreSetupSection { Name = "EU catalog" });

        //Assert
        var saved = Assert.Single(_saved.Values);
        Assert.Equal(("EU catalog", context.Store.Catalog), (saved.Name, saved.Id));
        Assert.Empty(repeat.Changes);
    }

    // The plan applies to the catalog read again: its new name and its gained languages, and a change made since stays.
    [Fact]
    public async Task ApplyAsync_RenamesAndAddsLanguages_ToTheCatalogReadAgain()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "Old name", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        var store = CreateStore();
        store.Languages = ["de-DE", "en-US"];
        var context = CreateContext(store);
        var plan = await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Name = "New name" });

        // A person adds a language in the back office between the plan and the apply.
        _saved[CatalogId].Languages.Add(new CatalogLanguage { LanguageCode = "it-IT" });

        //Act
        await CreatePart().ApplyAsync(context, plan);

        //Assert
        Assert.Equal("New name", _saved[CatalogId].Name);
        Assert.Equal(["de-DE", "it-IT", "en-US"], _saved[CatalogId].Languages.Select(x => x.LanguageCode));
    }

    // A reused catalog that gains nothing is not saved: the store's row, which basics saves, is the change.
    [Fact]
    public async Task ApplyAsync_AReusedCatalogThatGainsNothing_SavesNothing()
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        var context = CreateContext(CreateNewStore(), isNewStore: true);
        var plan = await CreatePart().PlanAsync(context, new CatalogStoreSetupSection { Id = MainId });

        //Act
        await CreatePart().ApplyAsync(context, plan);

        //Assert
        _catalogService.Verify(x => x.SaveChangesAsync(It.IsAny<IList<Catalog>>()), Times.Never);
    }

    // The state is the catalog's id and name: copied into a new store's setup, it reuses the catalog.
    [Fact]
    public async Task GetStateAsync_IsTheCatalogsIdAndName()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "EU catalog" };

        //Act
        var state = (CatalogStoreSetupSection)await CreatePart().GetStateAsync(CreateStore());

        //Assert
        Assert.Equal((CatalogId, "EU catalog"), (state.Id, state.Name));
    }

    // Every catalog is a choice for catalog.id: the catalogs stores sell from first, the most used first, then by name, in
    // whatever order the search returns them.
    [Fact]
    public async Task GetChoicesAsync_ListsEveryCatalog_TheMostUsedFirst_WithWhetherItIsVirtual()
    {
        //Arrange
        _saved["gamma"] = new Catalog { Id = "gamma", Name = "Gamma" };
        _saved["alpha"] = new Catalog { Id = "alpha", Name = "Alpha" };
        _saved["beta"] = new Catalog { Id = "beta", Name = "Beta", IsVirtual = true };
        _saved["zeta"] = new Catalog { Id = "zeta", Name = "Zeta" };
        _stores.AddRange([new Store { Id = "s1", Catalog = "zeta" }, new Store { Id = "s2", Catalog = "ZETA" }, new Store { Id = "s3", Catalog = "beta" }]);

        //Act
        var choices = await CreatePart().GetChoicesAsync();

        //Assert
        Assert.All(choices, x => Assert.Equal("catalog.id", x.Field));
        Assert.Equal(
            [
                ("zeta", "Zeta", "Physical catalog, used by 2 stores."),
                ("beta", "Beta", "Virtual catalog, used by 1 store."),
                ("alpha", "Alpha", "Physical catalog, used by no store."),
                ("gamma", "Gamma", "Physical catalog, used by no store."),
            ],
            choices.Select(x => (x.Value, x.Title, x.Description)));
    }

    // The save refuses a catalog without a default language, so the plan refuses it first: a new store without
    // languages, and a store whose missing catalog would be created again without a default.
    [Fact]
    public async Task PlanAsync_ACatalogWithoutADefaultLanguage_IsRefusedBeforeTheSave()
    {
        //Arrange
        var newStore = CreateNewStore();
        newStore.DefaultLanguage = null;
        newStore.Languages = [];
        var storeWithoutDefault = CreateStore();
        storeWithoutDefault.DefaultLanguage = null;
        storeWithoutDefault.Languages = [];
        const string expected = "A new catalog needs a default language, and the store has none: send basics.languages (the first becomes the default) or basics.default_language.";

        //Act
        var forNewStore = await CreatePart().PlanAsync(CreateContext(newStore, isNewStore: true), new CatalogStoreSetupSection { Name = "EU catalog" });
        var forMissingCatalog = await CreatePart().PlanAsync(CreateContext(storeWithoutDefault), new CatalogStoreSetupSection { Name = "EU catalog" });
        var forMissingCatalogById = await CreatePart().PlanAsync(CreateContext(storeWithoutDefault), new CatalogStoreSetupSection { Id = CatalogId, Name = "EU catalog" });

        //Assert
        Assert.Equal([expected], forNewStore.Errors);
        Assert.Equal([expected], forMissingCatalog.Errors);
        // The name was sent: no second error asks for it.
        Assert.Equal([expected], forMissingCatalogById.Errors);
        Assert.Null(newStore.Catalog);
    }

    // A new catalog gets the SEO record the catalogs API gives one: slug "catalog", title "Catalog", the default language.
    [Fact]
    public async Task PlanAsync_ANewCatalog_GetsTheDefaultSeoRecord()
    {
        //Act
        var plan = (CatalogStoreSetupPlan)await CreatePart().PlanAsync(CreateContext(CreateNewStore(), isNewStore: true), new CatalogStoreSetupSection { Name = "EU catalog" });

        //Assert
        var seoInfo = Assert.Single(plan.Catalog.SeoInfos);
        Assert.Equal(("catalog", "Catalog", "de-DE"), (seoInfo.SemanticUrl, seoInfo.PageTitle, seoInfo.LanguageCode));
    }

    [Fact]
    public void Permissions_AreCatalogsOwn_GrantedGlobally()
    {
        //Act
        var permissions = CreatePart().Permissions;

        //Assert
        Assert.Equal(("catalog:read", "catalog:update", "catalog:create", (string)null), (permissions.Read, permissions.Update, permissions.Create, permissions.ScopeType));
    }

    // The plan needs the permission of what the apply saves even when an override plans it without calling base, which
    // names the permission: catalog:create for a new catalog.
    [Fact]
    public async Task PlanAsync_APlanNewCatalogOverrideThatSkipsBase_StillNeedsCatalogCreate()
    {
        //Act
        var plan = (CatalogStoreSetupPlan)await CreateBaseSkippingPart().PlanAsync(CreateContext(CreateNewStore(), isNewStore: true), new CatalogStoreSetupSection { Name = "EU catalog" });

        //Assert
        Assert.True(plan.IsNewCatalog);
        Assert.Equal(["catalog:read", "catalog:create"], plan.RequiredPermissions);
    }

    // catalog:update for the languages a catalog gains.
    [Fact]
    public async Task PlanAsync_APlanLanguagesOverrideThatSkipsBase_StillNeedsCatalogUpdate()
    {
        //Arrange
        _saved[MainId] = new Catalog { Id = MainId, Name = "Main", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };
        var store = CreateNewStore();
        store.Languages = ["de-DE", "fr-FR"];

        //Act
        var plan = (CatalogStoreSetupPlan)await CreateBaseSkippingPart().PlanAsync(CreateContext(store, isNewStore: true), new CatalogStoreSetupSection { Id = MainId });

        //Assert
        Assert.Equal(["fr-FR"], plan.AddedLanguages);
        Assert.Equal(["catalog:read", "catalog:update"], plan.RequiredPermissions);
    }

    // catalog:update for a rename.
    [Fact]
    public async Task PlanAsync_ARenameOverrideThatSkipsBase_StillNeedsCatalogUpdate()
    {
        //Arrange
        _saved[CatalogId] = new Catalog { Id = CatalogId, Name = "Old name", Languages = [new CatalogLanguage { LanguageCode = "de-DE", IsDefault = true }] };

        //Act
        var plan = await CreateBaseSkippingPart().PlanAsync(CreateContext(CreateStore()), new CatalogStoreSetupSection { Name = "New name" });

        //Assert
        Assert.Equal(CatalogStoreSetupPart.NameField, Assert.Single(plan.Changes).Field);
        Assert.Equal(["catalog:update"], plan.RequiredPermissions);
    }

    private CatalogStoreSetupPart CreatePart()
    {
        return new CatalogStoreSetupPart(_catalogService.Object, CreateCatalogSearchService(), CreateStoreSearchService(), new StoreSetupFindings());
    }

    private BaseSkippingCatalogStoreSetupPart CreateBaseSkippingPart()
    {
        return new BaseSkippingCatalogStoreSetupPart(_catalogService.Object, CreateCatalogSearchService(), CreateStoreSearchService(), new StoreSetupFindings());
    }

    private ICatalogSearchService CreateCatalogSearchService()
    {
        var catalogSearchService = new Mock<ICatalogSearchService>();
        catalogSearchService
            .Setup(x => x.SearchAsync(It.IsAny<CatalogSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((CatalogSearchCriteria criteria, bool _) => new CatalogSearchResult
            {
                TotalCount = _saved.Count,
                Results = _saved.Values.Skip(criteria.Skip).Take(criteria.Take).Select(Copy).ToList(),
            });

        return catalogSearchService.Object;
    }

    private IStoreSearchService CreateStoreSearchService()
    {
        var storeSearchService = new Mock<IStoreSearchService>();
        storeSearchService
            .Setup(x => x.SearchAsync(It.IsAny<StoreSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((StoreSearchCriteria criteria, bool _) => new StoreSearchResult
            {
                TotalCount = _stores.Count,
                Results = _stores.Skip(criteria.Skip).Take(criteria.Take).ToList(),
            });

        return storeSearchService.Object;
    }

    private static Store CreateStore()
    {
        return new Store { Id = "eu-store", Name = "EU store", Catalog = CatalogId, DefaultLanguage = "de-DE", Languages = ["de-DE"] };
    }

    private static Store CreateNewStore()
    {
        return new Store { Id = "new-store", Name = "New store", DefaultLanguage = "de-DE", Languages = ["de-DE"] };
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
