using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.MCP.Models;

// A part's finding about a product as the tools return it (ProductFindingRows): the product's SKU, whether the finding
// keeps the product from being bought, and the back-office link in its fix.
public class ProductFinding : McpFinding
{
    public string Sku { get; set; }

    public bool BlocksSale { get; set; }
}
