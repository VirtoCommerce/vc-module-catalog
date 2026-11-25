using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// The findings of a product as every part writes them. Catalog registers ProductSetupFindings; a solution changes how
// every part writes them by registering its own.
public interface IProductSetupFindings
{
    // A finding about a product, on the product's screen in the back office; the part is its category. The caller fills
    // in what is wrong, why it matters, the figures and the fix.
    McpFinding ForProduct(CatalogProduct product, string part, string id, string severity);
}
