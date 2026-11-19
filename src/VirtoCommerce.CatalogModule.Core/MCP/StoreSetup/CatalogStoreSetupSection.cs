using System.ComponentModel;

namespace VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;

// Catalog's section of a store's setup, as an agent fills it: "catalog": { "name": "EU catalog" }. The description becomes
// the JSON schema that get_store_setup_options shows.
public class CatalogStoreSetupSection
{
    [Description("Name of the store's catalog. A new store gets a new catalog with this name; for an existing store, the " +
        "name renames the store's catalog. Required for a new store.")]
    public string Name { get; set; }
}
