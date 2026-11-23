using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;

// The plan of the catalog section: the store's catalog as the plan leaves it, which the check reads for the preview.
public class CatalogStoreSetupPlan : McpSetupPartPlan
{
    // The new catalog, the store's existing catalog with the planned name and languages, or the catalog the store switches
    // to with the languages it gains; null when the store has none.
    public Catalog Catalog { get; set; }

    // The catalog does not exist yet: a new store's new catalog, or the one a store points to that is missing.
    public bool IsNewCatalog { get; set; }

    // The languages an existing catalog gains: applying adds them to the catalog read again, so nothing else changes.
    public IList<string> AddedLanguages { get; set; } = [];
}
