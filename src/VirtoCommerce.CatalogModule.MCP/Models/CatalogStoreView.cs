using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CatalogModule.MCP.Models;

// A store as one user sees its catalogs (CatalogAccess.GetStoreViewAsync): every list and figure of the store's products
// covers the catalogs the user may read products of.
public class CatalogStoreView
{
    public Store Store { get; set; }

    // The store's catalog, physical or virtual.
    public Catalog StoreCatalog { get; set; }

    // The physical catalogs whose products the store sells and the user may read.
    public IList<Catalog> ReadableCatalogs { get; set; } = [];

    // The ids of the physical catalogs whose products the store sells and the user may not read. Never named or counted
    // in an answer; the search index queries exclude their products.
    public IList<string> OtherCatalogIds { get; set; } = [];

    // The store sells products of catalogs the user may not read.
    public bool HasOtherCatalogs => OtherCatalogIds.Count > 0;

    // Index figures over the readable catalogs are exact: no product of a readable catalog is also linked into a catalog
    // the user may not read, which the index queries would leave out.
    public bool IndexFiguresExact { get; set; } = true;
}
