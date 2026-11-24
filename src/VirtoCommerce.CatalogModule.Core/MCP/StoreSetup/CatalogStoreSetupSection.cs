using System.ComponentModel;

namespace VirtoCommerce.CatalogModule.Core.MCP.StoreSetup;

// Catalog's section of a store's setup, as an agent fills it: "catalog": { "id": "main" } to sell from an existing
// catalog, or "catalog": { "name": "EU catalog" } for a new one. The descriptions become the JSON schema that
// get_store_setup_options shows.
public class CatalogStoreSetupSection
{
    [Description("Id of an existing catalog, as get_store_setup_options lists the catalogs; one that is not listed can " +
        "still be sent by its id. A new store sells from it, shared with the stores that already do, and nothing is " +
        "created. For an existing store, another catalog's id switches the store to that catalog, a product catalog or a " +
        "virtual one; the preview says what the switch changes for customers.")]
    public string Id { get; set; }

    [Description("Name of the catalog. For a new store without id: a new, empty catalog with this name is created; the " +
        "preview warns, with the id, when a catalog already has the name. With id: that catalog's own name only, as " +
        "creating a store or switching its catalog never renames one. For an existing store without another catalog's " +
        "id: renames the store's catalog.")]
    public string Name { get; set; }
}
