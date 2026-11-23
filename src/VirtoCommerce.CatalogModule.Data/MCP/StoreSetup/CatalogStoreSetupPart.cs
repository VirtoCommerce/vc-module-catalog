using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;

// Catalog's part of a store's setup: the catalog a store sells from. A new store gets a new, empty catalog by its name,
// or sells from an existing catalog by its id, shared with the stores that already do. An existing store's catalog is
// renamed; which catalog a store sells from is changed in the back office. Whenever the section is planned, the catalog
// gains the store's languages it lacks, and never loses one. Store cannot create a catalog itself, as Catalog depends on
// Store: Catalog registers this part in its Module.Initialize with AddStoreSetupPart.
// A plan names the permissions it needs (RequiredPermissions), whether or not the store is new: catalog:read when it
// reads or shows a catalog, catalog:update when it changes one, and catalog:create when it creates one.
public class CatalogStoreSetupPart(
    ICatalogService catalogService,
    ICatalogSearchService catalogSearchService,
    IStoreSearchService storeSearchService,
    IStoreSetupFindings setupFindings)
    : IStoreSetupChoices, IStoreSetupState, IStoreSetupCheck, IStoreSetupSection
{
    public const string PartName = "catalog";

    // The catalogs table holds names of up to 128 characters.
    public const int MaxNameLength = 128;

    // The row that says, in words for the approver, which catalog the store sells from: a new one or an existing one.
    public const string StoreCatalogField = "store_catalog";

    // The rows of a rename and of the languages a catalog gains: the plan writes them, and the apply reads them back.
    public const string NameField = "name";
    public const string LanguagesField = "languages";

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

    // Every catalog, for the section's id: the catalogs stores sell from first, the most used first, then by name.
    public virtual async Task<IList<McpSetupChoice>> GetChoicesAsync()
    {
        var catalogs = await GetCatalogsAsync();
        var storeCounts = await GetStoreCountsAsync();

        return catalogs
            .Select(x => (Catalog: x, Stores: storeCounts.TryGetValue(x.Id, out var count) ? count : 0))
            .OrderByDescending(x => x.Stores)
            .ThenBy(x => x.Catalog.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => CreateChoice(x.Catalog, x.Stores))
            .ToList();
    }

    // The store's catalog by its id and name: copied into a new store's setup, the section reuses this catalog.
    public virtual async Task<object> GetStateAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var section = AbstractTypeFactory<CatalogStoreSetupSection>.TryCreateInstance();
        section.Id = store.Catalog;
        section.Name = (await GetCatalogAsync(store.Catalog))?.Name;

        return section;
    }

    // What is missing: the store's catalog, and the store's languages its catalog lacks. The catalog as the request plans
    // it counts, so a change of the store's languages sent without the catalog section shows what its catalog would lack.
    public virtual async Task<IList<McpFinding>> CheckAsync(StoreSetupContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var store = context.Store;
        var catalog = context.Plans.TryGetValue(PartName, out var plan) && plan is CatalogStoreSetupPlan { Catalog: not null } catalogPlan
            ? catalogPlan.Catalog
            : await GetCatalogAsync(store.Catalog);

        if (catalog == null)
        {
            // A new store gets its catalog from the section. The section refuses an existing store without a catalog id,
            // so only a person gives it one, in the back office.
            var noCatalogId = string.IsNullOrEmpty(store.Catalog);
            var fix = (noCatalogId, context.IsNewStore) switch
            {
                (false, _) => "Send the catalog section with a name to create the catalog again under the store's " +
                    "catalog id, or give the store an existing catalog in the back office.",
                (true, true) => "Send the catalog section with a name for a new, empty catalog, or with the id of an " +
                    "existing one.",
                (true, false) => "A person gives the store an existing catalog in the back office: " +
                    $"Stores > {store.Name ?? store.Id}, field Catalog.",
            };
            var notFound = CreateFinding(store, "catalog-not-found", McpFindingSeverities.Critical,
                "The store's catalog does not exist",
                "Customers find no products in the store.",
                noCatalogId ? "Catalog: none." : $"Catalog '{store.Catalog}': not found.",
                fix);
            notFound.ManualOnly = noCatalogId && !context.IsNewStore;

            return [notFound];
        }

        var missing = GetMissingLanguages(catalog, store);

        if (missing.Count == 0)
        {
            return [];
        }

        // Fixed on the catalog, not on the store's screen: the fix says where.
        var finding = CreateFinding(store, "catalog-lacks-store-languages", McpFindingSeverities.Warning,
            "The store's catalog lacks some of the store's languages",
            "Products cannot have names and descriptions in these languages, so customers see them in another one.",
            $"Missing in the catalog '{catalog.Name}': {string.Join(", ", missing)}.",
            $"Send the catalog section with the store's catalog id ({catalog.Id}): the catalog gains the store's languages. " +
                $"A person adds them in the back office too: Catalog > {catalog.Name}, Languages.");
        finding.Screen = null;

        return [finding];
    }

    // Plans without side effects. A new store: an existing catalog by its id, or a new, empty catalog by its name. An
    // existing store: renames its catalog. In both, the catalog gains the store's languages it lacks.
    public virtual async Task<McpSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        ArgumentNullException.ThrowIfNull(context);

        var catalogSection = (CatalogStoreSetupSection)section;
        var id = catalogSection.Id?.Trim().EmptyToNull();
        var name = catalogSection.Name?.Trim();
        var plan = AbstractTypeFactory<CatalogStoreSetupPlan>.TryCreateInstance();
        // The plan adds each permission as it reads, changes or creates a catalog; a refusal before that needs none.
        plan.RequiredPermissions = [];

        if (name != null && !IsValidName(name))
        {
            plan.Errors.Add($"The catalog needs a name of 1 to {MaxNameLength} characters.");
            return plan;
        }

        plan = context.IsNewStore
            ? await PlanNewStoreAsync(plan, context.Store, id, name)
            : await PlanExistingStoreAsync(plan, context.Store, id, name);

        // An override of the methods above that skips base names no permission, so the permissions of what ApplyAsync
        // saves are named here too: catalog:create for a new catalog, catalog:update for a rename or added languages.
        if (plan.IsNewCatalog)
        {
            plan.RequirePermission(Permissions.Create);
        }

        if (plan.Changes.Any(x => x.Field == NameField) || plan.AddedLanguages.Count > 0)
        {
            plan.RequirePermission(Permissions.Update);
        }

        return plan;
    }

    // Creates the new catalog, or applies the plan to the store's catalog read again: its new name and the languages it
    // gains, so a change made since the plan stays. A repeat changes nothing.
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

        var renamed = plan.Changes.Any(x => x.Field == NameField);

        if (!renamed && catalogPlan.AddedLanguages.Count == 0)
        {
            return;
        }

        var catalog = await GetCatalogAsync(catalogPlan.Catalog.Id)
            ?? throw new InvalidOperationException($"Catalog '{catalogPlan.Catalog.Id}' was not found.");

        if (renamed)
        {
            catalog.Name = catalogPlan.Catalog.Name;
        }

        AddLanguages(catalog, catalogPlan.AddedLanguages, context.Store.DefaultLanguage);

        await catalogService.SaveChangesAsync([catalog]);
    }

    protected virtual async Task<CatalogStoreSetupPlan> PlanNewStoreAsync(CatalogStoreSetupPlan plan, Store store, string id, string name)
    {
        if (id == null && name == null)
        {
            plan.Errors.Add("A new store needs its catalog: send name to create a new, empty catalog, or id to sell from an existing one.");
            return plan;
        }

        if (id == null)
        {
            if (!PlanNewCatalog(plan, store, name, $"a new, empty catalog '{name}'"))
            {
                return plan;
            }

            // The catalogs are read for one with the same name, found or not.
            plan.RequirePermission(Permissions.Read);
            var sameName = (await GetCatalogsAsync()).Where(x => x.Name.EqualsIgnoreCase(name)).Select(x => x.Id).ToList();

            if (sameName.Count > 0)
            {
                plan.Warnings.Add($"A catalog named '{name}' exists already ({string.Join(", ", sameName)}). To sell from it " +
                    "instead of a new, empty catalog, send its id instead of the name.");
            }

            return plan;
        }

        // Whether the catalog exists, and what it is, is the catalog's data.
        plan.RequirePermission(Permissions.Read);
        var catalog = await GetCatalogAsync(id);

        if (catalog == null)
        {
            plan.Errors.Add($"Catalog '{id}' does not exist. get_store_setup_options lists the catalogs.");
            return plan;
        }

        if (name != null && !IsSameName(catalog.Name, name))
        {
            plan.Errors.Add($"Creating a store never renames a catalog. To sell from the catalog '{catalog.Name}' ({catalog.Id}), " +
                $"leave out name; to create a new catalog named '{name}', leave out id.");
            return plan;
        }

        store.Catalog = catalog.Id;
        plan.Catalog = catalog;
        plan.AddChange(StoreCatalogField, null, DescribeExisting(catalog, await CountOtherStoresAsync(catalog.Id, store.Id)));
        PlanLanguages(plan, catalog, store);

        return plan;
    }

    protected virtual async Task<CatalogStoreSetupPlan> PlanExistingStoreAsync(CatalogStoreSetupPlan plan, Store store, string id, string name)
    {
        var storeName = store.Name ?? store.Id;

        // A store without a catalog id gets one in the back office: a catalog created here would never become the
        // store's, as Store saves only its own fields of an existing store. Refused before a catalog is read, so the
        // refusal needs no permission.
        if (string.IsNullOrEmpty(store.Catalog))
        {
            plan.Errors.Add($"Store '{storeName}' has no catalog. A store's catalog is set in the back office: " +
                $"Stores > {storeName}, field Catalog.");
            return plan;
        }

        // The store's catalog is read, and its name and languages are shown.
        plan.RequirePermission(Permissions.Read);
        var catalog = await GetCatalogAsync(store.Catalog);

        if (id != null && !id.EqualsIgnoreCase(store.Catalog))
        {
            var current = catalog == null ? $"'{store.Catalog}'" : $"'{catalog.Name}' ({catalog.Id})";
            plan.Errors.Add($"Store '{storeName}' sells from the catalog {current}. A store's catalog is changed in the back " +
                $"office: Stores > {storeName}, field Catalog.");
            return plan;
        }

        // The store points to a catalog that is missing: a name creates it again under that id.
        if (catalog == null)
        {
            if (name != null)
            {
                PlanNewCatalog(plan, store, name, $"a new, empty catalog '{name}', under the store's catalog id '{store.Catalog}'");
            }

            if (name == null && id != null)
            {
                plan.Errors.Add($"Catalog '{id}' does not exist. Send name to create it again under this id, or give the store " +
                    $"an existing catalog in the back office: Stores > {storeName}, field Catalog.");
            }

            return plan;
        }

        if (name != null && !IsSameName(catalog.Name, name))
        {
            plan.RequirePermission(Permissions.Update);
            plan.AddChange(NameField, catalog.Name, name);
            catalog.Name = name;

            var otherStores = await CountOtherStoresAsync(catalog.Id, store.Id);

            if (otherStores > 0)
            {
                plan.Warnings.Add(otherStores == 1
                    ? "1 other store sells from this catalog and shows the new name."
                    : $"{otherStores} other stores sell from this catalog and show the new name.");
            }
        }

        plan.Catalog = catalog;
        PlanLanguages(plan, catalog, store);

        return plan;
    }

    // A new, empty catalog for the store, under the store's catalog id when it has one, as a row of the plan; false when the
    // store has no default language, which HasDefaultLanguage refuses.
    protected virtual bool PlanNewCatalog(CatalogStoreSetupPlan plan, Store store, string name, string row)
    {
        if (!HasDefaultLanguage(plan, store))
        {
            return false;
        }

        plan.RequirePermission(Permissions.Create);
        plan.Catalog = CreateCatalog(store, name);
        plan.IsNewCatalog = true;
        store.Catalog = plan.Catalog.Id;
        plan.AddChange(StoreCatalogField, null, row);

        return true;
    }

    // A catalog is saved with a default language, and the store's default becomes the new catalog's: a store without one
    // is refused here, before anything is saved.
    protected virtual bool HasDefaultLanguage(CatalogStoreSetupPlan plan, Store store)
    {
        if (!string.IsNullOrEmpty(store.DefaultLanguage))
        {
            return true;
        }

        plan.Errors.Add("A new catalog needs a default language, and the store has none: send basics.languages (the first " +
            "becomes the default) or basics.default_language.");

        return false;
    }

    // The store's languages the catalog lacks become the catalog's, as a row of the plan; nothing is removed, as other
    // stores and existing texts may depend on a language.
    protected virtual void PlanLanguages(CatalogStoreSetupPlan plan, Catalog catalog, Store store)
    {
        var missing = GetMissingLanguages(catalog, store);

        if (missing.Count == 0)
        {
            return;
        }

        var before = (catalog.Languages ?? []).Select(x => x.LanguageCode).ToList();
        AddLanguages(catalog, missing, store.DefaultLanguage);
        plan.RequirePermission(Permissions.Update);
        plan.AddedLanguages = missing;
        plan.AddChange(LanguagesField, before, catalog.Languages.Select(x => x.LanguageCode).ToList());
    }

    // The store's languages, its default first, that the catalog lacks. None for a virtual catalog: its texts live in the
    // physical catalogs behind it.
    protected virtual IList<string> GetMissingLanguages(Catalog catalog, Store store)
    {
        if (catalog.IsVirtual)
        {
            return [];
        }

        return GetStoreLanguages(store)
            .Where(code => !(catalog.Languages ?? []).Any(x => x.LanguageCode.EqualsIgnoreCase(code)))
            .ToList();
    }

    // A catalog without a default language takes the store's default as its own.
    protected virtual void AddLanguages(Catalog catalog, IEnumerable<string> languageCodes, string defaultLanguage)
    {
        catalog.Languages = catalog.Languages?.ToList() ?? [];

        foreach (var code in languageCodes.Where(code => !catalog.Languages.Any(x => x.LanguageCode.EqualsIgnoreCase(code))))
        {
            catalog.Languages.Add(CreateLanguage(code, isDefault: catalog.DefaultLanguage == null && code.EqualsIgnoreCase(defaultLanguage)));
        }
    }

    // A new catalog under the store's catalog id when the store has one, or a new id, with every language of the store and
    // the store's default as its default.
    protected virtual Catalog CreateCatalog(Store store, string name)
    {
        var catalog = AbstractTypeFactory<Catalog>.TryCreateInstance();
        catalog.Id = string.IsNullOrEmpty(store.Catalog) ? Guid.NewGuid().ToString("N") : store.Catalog;
        catalog.Name = name;
        catalog.Languages = GetStoreLanguages(store)
            .Select(code => CreateLanguage(code, isDefault: code.EqualsIgnoreCase(store.DefaultLanguage)))
            .ToList();
        AbstractTypeFactory<CatalogSeoInfoDefaults>.TryCreateInstance().EnsureSeoInfo(catalog);

        return catalog;
    }

    protected virtual CatalogLanguage CreateLanguage(string languageCode, bool isDefault)
    {
        var language = AbstractTypeFactory<CatalogLanguage>.TryCreateInstance();
        language.LanguageCode = languageCode;
        language.IsDefault = isDefault;

        return language;
    }

    // "the existing catalog 'Main' (main), shared with 2 other stores": what the approver reads for a reused catalog.
    protected virtual string DescribeExisting(Catalog catalog, int otherStores)
    {
        var kind = catalog.IsVirtual ? "virtual catalog" : "catalog";
        var sharing = otherStores switch
        {
            0 => "used by no other store",
            1 => "shared with 1 other store",
            _ => $"shared with {otherStores} other stores",
        };

        return $"the existing {kind} '{catalog.Name}' ({catalog.Id}), {sharing}";
    }

    protected virtual McpSetupChoice CreateChoice(Catalog catalog, int stores)
    {
        var choice = AbstractTypeFactory<McpSetupChoice>.TryCreateInstance();
        choice.Field = $"{PartName}.id";
        choice.Value = catalog.Id;
        choice.Title = catalog.Name;
        choice.Description = $"{(catalog.IsVirtual ? "Virtual" : "Physical")} catalog, used by {stores switch { 0 => "no store", 1 => "1 store", _ => $"{stores} stores" }}.";

        return choice;
    }

    protected virtual McpFinding CreateFinding(Store store, string id, string severity, string title, string why, string evidence, string fix)
    {
        var finding = setupFindings.ForStore(store, PartName, id, severity);
        finding.Title = title;
        finding.Why = why;
        finding.Evidence = evidence;
        finding.Fix = fix;

        return finding;
    }

    // How many stores sell from each catalog, the store with the given id left out. Store's search has no catalog filter,
    // and stores are few, so they are read in pages; only planning the section and listing the choices read them. Every
    // store counts, one the user may not read too: how widely a catalog is shared is the catalog's fact, which the user
    // needs before renaming it.
    protected virtual async Task<IDictionary<string, int>> GetStoreCountsAsync(string exceptStoreId = null)
    {
        var stores = await storeSearchService.SearchAllNoCloneAsync(AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance());

        return stores
            .Where(x => !string.IsNullOrEmpty(x.Catalog) && !x.Id.EqualsIgnoreCase(exceptStoreId))
            .GroupBy(x => x.Catalog, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
    }

    protected virtual async Task<int> CountOtherStoresAsync(string catalogId, string storeId)
    {
        return (await GetStoreCountsAsync(storeId)).TryGetValue(catalogId, out var count) ? count : 0;
    }

    protected virtual async Task<IList<Catalog>> GetCatalogsAsync()
    {
        return await catalogSearchService.SearchAllNoCloneAsync(AbstractTypeFactory<CatalogSearchCriteria>.TryCreateInstance());
    }

    // A copy of the catalog, or null.
    protected virtual async Task<Catalog> GetCatalogAsync(string catalogId)
    {
        return string.IsNullOrEmpty(catalogId) ? null : await catalogService.GetByIdAsync(catalogId);
    }

    // A catalog's name: 1 to 128 characters, without the spaces around it.
    protected virtual bool IsValidName(string name)
    {
        return name.Length is > 0 and <= MaxNameLength;
    }

    // A name sent is the catalog's name when they differ only by spaces around them, so a state sent back renames nothing.
    protected virtual bool IsSameName(string catalogName, string name)
    {
        return string.Equals(catalogName?.Trim(), name, StringComparison.Ordinal);
    }

    // The store's languages, its default first.
    protected virtual IList<string> GetStoreLanguages(Store store)
    {
        return new[] { store.DefaultLanguage }
            .Concat(store.Languages ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
