using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Tests.MCP.StoreSetup;

// A solution's catalog part whose overrides plan a new catalog, the languages a catalog gains and a rename without
// calling the base methods, so they name no permission, not even catalog:read for the catalog they read. Each sets what
// the apply reads: the catalog, IsNewCatalog or AddedLanguages, and the row.
public class BaseSkippingCatalogStoreSetupPart(
    ICatalogService catalogService,
    ICatalogSearchService catalogSearchService,
    IStoreService storeService,
    IStoreSearchService storeSearchService,
    IStoreSetupFindings setupFindings)
    : CatalogStoreSetupPart(catalogService, catalogSearchService, storeService, storeSearchService, setupFindings)
{
    protected override bool PlanNewCatalog(CatalogStoreSetupPlan plan, Store store, string name, string row)
    {
        plan.Catalog = CreateCatalog(store, name);
        plan.IsNewCatalog = true;
        store.Catalog = plan.Catalog.Id;
        plan.AddChange(StoreCatalogField, null, row);

        return true;
    }

    protected override void PlanLanguages(CatalogStoreSetupPlan plan, Catalog catalog, Store store)
    {
        var missing = GetMissingLanguages(catalog, store);

        if (missing.Count > 0)
        {
            plan.AddedLanguages = missing;
            plan.AddChange(LanguagesField, null, missing);
        }
    }

    // An existing store's catalog renamed, as the base method renames it.
    protected override async Task<CatalogStoreSetupPlan> PlanExistingStoreAsync(CatalogStoreSetupPlan plan, Store store, string id, string name)
    {
        var catalog = await GetCatalogAsync(store.Catalog);
        plan.AddChange(NameField, catalog.Name, name);
        catalog.Name = name;
        plan.Catalog = catalog;

        return plan;
    }
}
