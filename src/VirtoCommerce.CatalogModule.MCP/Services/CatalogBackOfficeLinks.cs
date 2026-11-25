using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;
using VirtoCommerce.MCP.Core.Services;

namespace VirtoCommerce.CatalogModule.MCP.Services;

// The links of the back-office screens the tools point to for what they do not cover.
public class CatalogBackOfficeLinks(IMcpLinkResolver linkResolver)
{
    // The Catalog screen. It takes no catalog or product id, so the link opens the list of catalogs.
    public const string CatalogRoute = "/#!/workspace/catalog";

    // The link to the Catalog screen; null outside an MCP request and when the scope has no public origin.
    public virtual Task<string> GetCatalogLinkAsync()
    {
        return linkResolver.GetLinkAsync(CatalogRoute);
    }

    // The link to the Search index screen, where an index is built.
    public virtual Task<string> GetSearchIndexLinkAsync()
    {
        return linkResolver.GetLinkAsync(ProductIndexingTexts.SearchIndexRoute);
    }
}
