using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;

public static class ProductSetupPermissions
{
    // The scope type that limits Catalog's permissions to catalogs: the catalog authorization handler checks it on each
    // product's own catalog.
    public const string CatalogScopeType = "SelectedCatalogScope";

    // Catalog's product permissions, as the products API checks them.
    public static McpSetupPermissions ForProducts()
    {
        var permissions = AbstractTypeFactory<McpSetupPermissions>.TryCreateInstance();
        permissions.Read = CatalogPermissions.ProductsRead;
        permissions.Update = CatalogPermissions.ProductsUpdate;
        permissions.Create = CatalogPermissions.ProductsCreate;
        permissions.ScopeType = CatalogScopeType;

        return permissions;
    }
}
