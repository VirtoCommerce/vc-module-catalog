using System;
using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// The catalogs a store sells from: its own catalog, physical or virtual, and the physical catalogs whose products it sells,
// which are the store's catalog when it is physical and every catalog that links categories or products into the store's
// catalog, or into one of those, at any depth. All copies.
public class StoreCatalogs
{
    // Null when the store has no catalog or its catalog was not found.
    public Catalog StoreCatalog { get; set; }

    public IList<Catalog> Catalogs { get; set; } = [];

    // For each catalog above, the physical catalogs that link categories or products into it. A count from the search
    // index filtered by the catalogs a user may read is exact only when no readable catalog holds a link from another one.
    public IDictionary<string, IList<string>> LinkedFrom { get; set; } = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
}
