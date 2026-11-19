using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;

// Catalog's part of a store's setup: the store's catalog, by its name. A new store gets a new catalog, and an existing
// store's catalog is renamed. Store cannot create a catalog itself, as Catalog depends on Store: Catalog registers this
// part in its Module.Initialize with AddStoreSetupPart.
public class CatalogStoreSetupPart(ICatalogService catalogService) : IStoreSetupState, IStoreSetupCheck, IStoreSetupSection
{
    public const string PartName = "catalog";

    // The catalogs table holds names of up to 128 characters.
    public const int MaxNameLength = 128;

    public virtual string Name => PartName;

    public virtual string Title => "Catalog";

    // Catalog's own permissions. Their scopes limit them to catalogs, not stores, so only a global grant counts.
    public virtual McpSetupPermissions Permissions
    {
        get
        {
            var permissions = AbstractTypeFactory<McpSetupPermissions>.TryCreateInstance();
            permissions.Read = CatalogPermissions.Read;
            permissions.Update = CatalogPermissions.Update;
            permissions.Create = CatalogPermissions.Create;

            return permissions;
        }
    }

    public virtual Type SectionType => typeof(CatalogStoreSetupSection);

    // A store is saved with its catalog: the column is required.
    public virtual bool IsRequiredForNewStore => true;

    // The store's catalog, by its name.
    public virtual async Task<object> GetStateAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var section = AbstractTypeFactory<CatalogStoreSetupSection>.TryCreateInstance();
        section.Name = (await GetCatalogAsync(store.Catalog))?.Name;

        return section;
    }

    // What is missing: the store's catalog. When the request plans the catalog section, the catalog it creates counts.
    public virtual async Task<IList<McpFinding>> CheckAsync(StoreSetupContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var store = context.Store;

        if (context.Plans.TryGetValue(PartName, out var plan) && plan is CatalogStoreSetupPlan { IsNewCatalog: true })
        {
            return [];
        }

        if (await GetCatalogAsync(store.Catalog) != null)
        {
            return [];
        }

        var finding = StoreSetupFindings.ForStore(store, PartName, "catalog-not-found", McpFindingSeverities.Critical);
        finding.Title = "The store's catalog does not exist";
        finding.Why = "Customers find no products in the store.";
        finding.Evidence = string.IsNullOrEmpty(store.Catalog) ? "Catalog: none." : $"Catalog '{store.Catalog}': not found.";
        finding.Fix = "Give the store a catalog by its name.";

        return [finding];
    }

    // Plans without side effects: the catalog to create, or the store's catalog with its new name. The planned store
    // points at a new catalog's id, so the store is saved with it and the catalog is created right after; a store whose
    // catalog is missing keeps its catalog id, and the catalog is created under it.
    public virtual async Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var name = ((CatalogStoreSetupSection)section).Name?.Trim();
        var plan = AbstractTypeFactory<CatalogStoreSetupPlan>.TryCreateInstance();

        // A field that is left out changes nothing.
        if (name == null)
        {
            return plan;
        }

        if (name.Length == 0 || name.Length > MaxNameLength)
        {
            plan.Errors.Add($"The catalog needs a name of 1 to {MaxNameLength} characters.");
            return plan;
        }

        var store = context.Store;
        var catalog = await GetCatalogAsync(store.Catalog);

        if (catalog == null)
        {
            plan.Catalog = CreateCatalog(store, name);
            plan.IsNewCatalog = true;
            plan.AddChange("name", null, name);
            store.Catalog = plan.Catalog.Id;

            return plan;
        }

        if (!string.Equals(catalog.Name, name, StringComparison.Ordinal))
        {
            plan.AddChange("name", catalog.Name, name);
            catalog.Name = name;
            plan.Catalog = catalog;
        }

        return plan;
    }

    // Creates the new catalog, or renames the store's catalog read again, so a repeat changes nothing.
    public virtual async Task ApplyAsync(StoreSetupContext context, McpSetupPartPlan plan)
    {
        var catalogPlan = (CatalogStoreSetupPlan)plan;

        if (catalogPlan.Catalog == null)
        {
            return;
        }

        if (catalogPlan.IsNewCatalog)
        {
            await catalogService.SaveChangesAsync([catalogPlan.Catalog]);
            return;
        }

        var catalog = await GetCatalogAsync(catalogPlan.Catalog.Id)
            ?? throw new InvalidOperationException($"Catalog '{catalogPlan.Catalog.Id}' was not found.");
        catalog.Name = catalogPlan.Catalog.Name;

        await catalogService.SaveChangesAsync([catalog]);
    }

    // A new catalog under the store's catalog id when the store has one, or a new id; the store's default language is the
    // catalog's.
    protected virtual Catalog CreateCatalog(Store store, string name)
    {
        var catalog = AbstractTypeFactory<Catalog>.TryCreateInstance();
        catalog.Id = string.IsNullOrEmpty(store.Catalog) ? Guid.NewGuid().ToString("N") : store.Catalog;
        catalog.Name = name;
        catalog.Languages = [];

        if (!string.IsNullOrEmpty(store.DefaultLanguage))
        {
            var language = AbstractTypeFactory<CatalogLanguage>.TryCreateInstance();
            language.LanguageCode = store.DefaultLanguage;
            language.IsDefault = true;
            catalog.Languages.Add(language);
        }

        return catalog;
    }

    // A copy of the catalog, or null.
    protected virtual async Task<Catalog> GetCatalogAsync(string catalogId)
    {
        return string.IsNullOrEmpty(catalogId) ? null : await catalogService.GetByIdAsync(catalogId);
    }
}
