using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

// A module's part of a product as a store sells it: Catalog's "product" (content, category, flags), Pricing's "price",
// Inventory's "stock". A part implements the duties it covers (IProductSetupState, IProductSetupCheck), and a module
// registers it with AddProductSetupPart. A part does not choose its place: Catalog's part comes first, then the other
// parts by name.
public interface IProductSetupPart
{
    // The key of the part's section in a product row and the category of its findings, such as "product" or "price".
    string Name { get; }

    string Title { get; }

    // The permissions of the part's module. Catalog's are checked on each product's own catalog; a part whose module
    // cannot limit its permissions (no ScopeType) counts them only when they are granted globally.
    McpSetupPermissions Permissions { get; }
}
