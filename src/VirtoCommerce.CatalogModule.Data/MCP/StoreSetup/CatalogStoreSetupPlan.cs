using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.Data.MCP.StoreSetup;

// The plan of the catalog section: the catalog to create, or the store's catalog with its new name; null when nothing
// changes.
public class CatalogStoreSetupPlan : McpSetupPartPlan
{
    public Catalog Catalog { get; set; }

    // The catalog does not exist yet: a new store's, or the one a store points to that is missing.
    public bool IsNewCatalog { get; set; }
}
