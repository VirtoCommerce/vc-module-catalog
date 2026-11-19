using System.ComponentModel;

namespace VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;

// Catalog's section of a store's setup, as an agent fills it: "catalog": { "id": "main" } to sell from an existing
// catalog, or "catalog": { "name": "EU catalog" } for a new one. The descriptions become the JSON schema that
// get_store_setup_options shows.
public class CatalogStoreSetupSection
{
    [Description("Id of an existing catalog, as get_store_setup_options lists the catalogs: a new store sells from it, " +
        "shared with the stores that already do, and nothing is created. A catalog that is not listed can still be sent " +
        "by its id. For an existing store, only the id of its own catalog: a store's catalog is changed in the back office.")]
    public string Id { get; set; }

    [Description("Name of the catalog. For a new store without id: a new, empty catalog with this name is created; the " +
        "preview warns, with the id, when a catalog already has the name. With id: the catalog's own name only, as " +
        "creating a store never renames a catalog. For an existing store: renames the store's catalog.")]
    public string Name { get; set; }
}
