using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// A product as a store sells it, across the parts that modules register. It checks no permissions, as no service does:
// the tools give it only the products the user may work on, and check each part's permissions.
public interface IProductSetupService
{
    // Every registered part: Catalog's own part first, then the others by name.
    IList<IProductSetupPart> GetParts();

    // The store's catalog and the physical catalogs whose products the store sells.
    Task<StoreCatalogs> GetStoreCatalogsAsync(Store store);

    // Each part's state of the products in the store: part name > product id > state.
    Task<IDictionary<string, IDictionary<string, object>>> GetStatesAsync(Store store, IList<string> productIds);

    // What keeps the products from being bought in the store, from every part that checks. A part whose check fails gives
    // one critical finding per product.
    Task<IList<McpFinding>> CheckAsync(Store store, IList<string> productIds);

    // Whether a finding keeps the product from being bought.
    bool BlocksSale(McpFinding finding);
}
