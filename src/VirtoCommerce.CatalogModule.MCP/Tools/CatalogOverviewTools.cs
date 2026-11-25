using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.CatalogModule.MCP.Services;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.MCP.Tools;

// Where a catalog manager starts: the stores, the catalogs they sell from, and exact figures of their products in the
// search index.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(CatalogPermissions.Read, StorePermissions.Read)]
public class CatalogOverviewTools(
    CatalogAccess access,
    CatalogReach reach,
    StoreProductIndex productIndex,
    IStoreSearchService storeSearchService,
    IMcpPageSizeResolver pageSizeResolver,
    ILogger<CatalogOverviewTools> logger)
{
    public const string OtherCatalogsNote = "This store also sells products of catalogs you may not read: they are not shown or counted.";

    public const string NotCountedNote = "Exact figures from the search index, which the storefront reads. Not counted here: products " +
        "saved but not in the index yet, and whether products can be bought, which depends on the store's prices and stock. " +
        "find_products_not_for_sale checks that exactly, by SKU or one window of products at a time.";

    [McpServerTool(Name = "get_catalog_overview", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Start here. The stores you manage, each with its catalog (physical or virtual), the physical catalogs whose " +
        "products it sells and which stores sell from each, and exact figures of its products in the search index, which the " +
        "storefront reads: in the index, listed, hidden, outside their dates. Figures and lists cover the catalogs you may " +
        "read. Then find_products_not_for_sale says what keeps products from sale.")]
    public async Task<object> GetCatalogOverview(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, for one store; left out, the stores you manage, a page at a time.")] string store_id = null,
        [Description("Stores to skip, for the next page.")] int? skip = null,
        [Description("Stores per page.")] int? take = null)
    {
        var user = context.User;
        var paging = await pageSizeResolver.GetPagingAsync(skip, take);
        IList<Store> stores = [];
        var totalCount = 0;

        if (string.IsNullOrWhiteSpace(store_id))
        {
            (stores, totalCount) = await SearchReadableStoresAsync(user, paging);
        }
        else
        {
            stores = [await access.GetReadableStoreAsync(user, store_id)];
            totalCount = 1;
        }

        var storesByCatalog = await reach.GetStoresByCatalogAsync();
        var rows = new List<object>();

        foreach (var store in stores)
        {
            rows.Add(await GetStoreRowAsync(user, store, storesByCatalog));
        }

        return new
        {
            stores = McpPage<object>.Create(rows, totalCount, paging),
            figures = NotCountedNote,
            next_step = "find_products_not_for_sale with a store_id lists the products that cannot be bought and why; search_products " +
                "finds products; get_catalog_structure shows a catalog's categories and properties.",
        };
    }

    // The stores the user may read, ordered by name: every store with a global grant, the granted ones otherwise.
    protected virtual async Task<(IList<Store> Stores, int TotalCount)> SearchReadableStoresAsync(ClaimsPrincipal user, McpPaging paging)
    {
        var grant = user.GetMcpPermissionGrant(StorePermissions.Read);
        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance().WithPaging(paging);
        criteria.Sort = nameof(Store.Name);

        if (grant == null)
        {
            return ([], 0);
        }

        if (!grant.IsGlobal)
        {
            var storeIds = grant.GetScopeValues(CatalogAccess.StoreScopeType);

            // An empty id list would not filter the search at all.
            if (storeIds.Count == 0)
            {
                return ([], 0);
            }

            criteria.ObjectIds = storeIds;
        }

        var result = await storeSearchService.SearchNoCloneAsync(criteria);

        return (result.Results, result.TotalCount);
    }

    protected virtual async Task<object> GetStoreRowAsync(ClaimsPrincipal user, Store store, IDictionary<string, IList<Store>> storesByCatalog)
    {
        CatalogStoreView view;

        try
        {
            view = await access.GetStoreViewAsync(user, store);
        }
        catch (McpException ex)
        {
            // A store without a catalog is still listed, with the reason.
            return new { store_id = store.Id, name = store.Name, problem = ex.Message };
        }

        // The store's own catalog is named only to a user who may read it.
        var catalogReadable = await access.IsAllowedAsync(user, view.StoreCatalog, CatalogPermissions.Read);

        return new
        {
            store_id = store.Id,
            name = store.Name,
            catalog = catalogReadable ? new { id = view.StoreCatalog.Id, name = view.StoreCatalog.Name, @virtual = view.StoreCatalog.IsVirtual } : null,
            catalog_not_shown = catalogReadable ? null : "The store's own catalog is not among the catalogs you may read.",
            sells_from = view.ReadableCatalogs.Select(x => new
            {
                catalog_id = x.Id,
                name = x.Name,
                sold_by = reach.Describe(user, storesByCatalog, x.Id),
            }).ToList(),
            other_catalogs = view.HasOtherCatalogs ? OtherCatalogsNote : null,
            products = await GetFiguresAsync(view),
        };
    }

    // The store's products in the search index, each figure one count. Left out when it would not be exact.
    protected virtual async Task<object> GetFiguresAsync(CatalogStoreView view)
    {
        if (!view.IndexFiguresExact)
        {
            return new
            {
                not_counted = "Some products of these catalogs are also linked into catalogs you may not read, so figures from the " +
                    "search index would fall short: they are left out. find_products_not_for_sale with skus checks products exactly.",
            };
        }

        try
        {
            var now = DateTime.UtcNow;

            return new
            {
                in_search_index = await productIndex.CountAsync(view),
                listed = await productIndex.CountAsync(view, productIndex.GetListedFilter(now)),
                hidden = await productIndex.CountAsync(view, productIndex.GetHiddenFilter()),
                outside_dates = await productIndex.CountAsync(view, productIndex.GetOutsideDatesFilter(now)),
            };
        }
        catch (Exception ex) when (ex is not McpException)
        {
            // The answer never carries an exception's text: it may hold a third party's trace or a secret.
            logger.LogError(ex, "The search index could not be read for the store {StoreId}", view.Store.Id);

            return new { not_counted = "The search index could not be read; the platform log has the details." };
        }
    }
}
