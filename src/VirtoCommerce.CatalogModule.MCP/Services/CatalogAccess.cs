using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.MCP.Services;

// Who may work on what, by the platform's own rules, with the user of the call: the requester in a dry run, the approver
// in an approved run. Three gates:
// 1. A store, by Store's rule: store:read granted globally or for the store (StoreSelectedScope).
// 2. Catalogs, categories, properties and products: the call the catalog API makes, IAuthorizationService with
//    CatalogAuthorizationRequirement, whose handler limits a permission to catalogs (SelectedCatalogScope) and falls back
//    from the category and product permissions to the general ones.
// 3. Prices, stock and centers: their modules cannot limit permissions, so they count only when granted globally; they
//    are reached only through the first two gates.
public class CatalogAccess(
    IAuthorizationService authorizationService,
    IStoreService storeService,
    ICatalogService catalogService,
    IProductSetupService productSetupService)
{
    public const string StoreScopeType = "StoreSelectedScope";

    // Gate 1: the store, with McpForbiddenException when the user may not read it.
    public virtual async Task<Store> GetReadableStoreAsync(ClaimsPrincipal user, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            throw new McpException("store_id is required. get_catalog_overview lists the stores you manage.");
        }

        if (!IsGranted(user, StorePermissions.Read, StoreScopeType, storeId))
        {
            throw new McpForbiddenException($"Your {StorePermissions.Read} permission does not cover store '{storeId}'.");
        }

        return await storeService.GetByIdAsync(storeId)
            ?? throw new McpException($"Store '{storeId}' was not found. get_catalog_overview lists the stores you manage.");
    }

    // The store and the catalogs it sells from, as the user may see them: the lists and figures of the store's products
    // cover the physical catalogs whose products the user may read.
    public virtual async Task<CatalogStoreView> GetStoreViewAsync(ClaimsPrincipal user, string storeId)
    {
        var store = await GetReadableStoreAsync(user, storeId);

        return await GetStoreViewAsync(user, store);
    }

    public virtual async Task<CatalogStoreView> GetStoreViewAsync(ClaimsPrincipal user, Store store)
    {
        var storeCatalogs = await productSetupService.GetStoreCatalogsAsync(store);

        if (storeCatalogs.StoreCatalog == null)
        {
            throw new McpException($"Store '{store.Id}' has no catalog, or its catalog was not found: give it one with apply_store_setup " +
                "and the 'catalog' section.");
        }

        var readableIds = await GetReadableCatalogIdsAsync(user, storeCatalogs.Catalogs.Select(x => x.Id).ToList());
        var view = AbstractTypeFactory<CatalogStoreView>.TryCreateInstance();
        view.Store = store;
        view.StoreCatalog = storeCatalogs.StoreCatalog;
        view.ReadableCatalogs = storeCatalogs.Catalogs.Where(x => readableIds.ContainsIgnoreCase(x.Id)).ToList();
        view.OtherCatalogIds = storeCatalogs.Catalogs.Select(x => x.Id).Where(x => !readableIds.ContainsIgnoreCase(x)).ToList();
        // An index query leaves out every product linked into a catalog the user may not read, so a readable catalog whose
        // products such a catalog links makes the figures fall short.
        view.IndexFiguresExact = !view.OtherCatalogIds.Any(other =>
            storeCatalogs.LinkedFrom.TryGetValue(other, out var linking) && linking.Any(readableIds.ContainsIgnoreCase));

        return view;
    }

    // The catalogs among catalogIds whose products the user may read: the handler's narrowing of a catalog search, with
    // catalog:products:read and its fallback to catalog:read, as the catalog API lists products.
    public virtual async Task<IList<string>> GetReadableCatalogIdsAsync(ClaimsPrincipal user, IList<string> catalogIds, string permission = CatalogPermissions.ProductsRead)
    {
        // An empty list would ask for every catalog the user may read.
        if (catalogIds.IsNullOrEmpty())
        {
            return [];
        }

        var criteria = AbstractTypeFactory<CatalogSearchCriteria>.TryCreateInstance();
        criteria.CatalogIds = catalogIds.ToArray();

        var result = await authorizationService.AuthorizeAsync(user, criteria, new CatalogAuthorizationRequirement(permission));

        return result.Succeeded ? (criteria.CatalogIds ?? []).ToList() : [];
    }

    // Gate 2 for any resource the catalog handler knows: a catalog, an object with a catalog id (category, product,
    // property, link) or a list of them.
    public virtual async Task<bool> IsAllowedAsync(ClaimsPrincipal user, object resource, string permission)
    {
        var result = await authorizationService.AuthorizeAsync(user, resource, new CatalogAuthorizationRequirement(permission));

        return result.Succeeded;
    }

    // refusal: a text that names only what the user sent.
    public virtual async Task EnsureAllowedAsync(ClaimsPrincipal user, object resource, string permission, string refusal)
    {
        if (!await IsAllowedAsync(user, resource, permission))
        {
            throw new McpForbiddenException(refusal);
        }
    }

    // The objects of a list that the user may work on with the permission, each checked on its own catalog.
    public virtual async Task<IList<T>> FilterAllowedAsync<T>(ClaimsPrincipal user, IEnumerable<T> items, string permission)
        where T : IHasCatalogId
    {
        var result = new List<T>();

        foreach (var item in items ?? [])
        {
            if (await IsAllowedAsync(user, item, permission))
            {
                result.Add(item);
            }
        }

        return result;
    }

    // A catalog the user may read, by its id: an unknown id and one the user may not read get the same answer, so the
    // answer tells nothing about catalogs the user may not read.
    public virtual async Task<Catalog> GetReadableCatalogAsync(ClaimsPrincipal user, string catalogId)
    {
        if (string.IsNullOrWhiteSpace(catalogId))
        {
            throw new McpException("catalog_id is required. get_catalog_overview names the catalogs of the stores you manage.");
        }

        var catalog = await catalogService.GetByIdAsync(catalogId.Trim());

        if (catalog == null || !await IsAllowedAsync(user, catalog, CatalogPermissions.Read))
        {
            throw new McpException($"Catalog '{catalogId}' is not among the catalogs you may read. get_catalog_overview names the catalogs of the " +
                "stores you manage.");
        }

        return catalog;
    }

    // Whether the user may read a part's state and findings: Catalog's part on the product's own catalog, any other part
    // with its read permission granted globally, as its module cannot limit it.
    public virtual async Task<bool> CanReadPartAsync(ClaimsPrincipal user, IProductSetupPart part, CatalogProduct product)
    {
        var permission = part.Permissions?.Read;

        if (string.IsNullOrEmpty(permission))
        {
            return false;
        }

        return part.Permissions.ScopeType.EqualsIgnoreCase(ProductSetupPermissions.CatalogScopeType)
            ? await IsAllowedAsync(user, product, permission)
            : IsGranted(user, permission, scopeType: null, value: null);
    }

    // What a user needs to see a part's state and findings: "needs pricing:read". A part whose module names no read
    // permission shows to no one.
    public virtual string GetReadRequirement(IProductSetupPart part)
    {
        var permission = part.Permissions?.Read;

        return string.IsNullOrEmpty(permission) ? "its module names no permission to read it" : $"needs {permission}";
    }

    // A permission whose module cannot limit it (no scope type) counts only when it is granted globally.
    public virtual bool IsGranted(ClaimsPrincipal user, string permission, string scopeType, string value)
    {
        var grant = user.GetMcpPermissionGrant(permission);

        return grant != null && (string.IsNullOrEmpty(scopeType) ? grant.IsGlobal : grant.Allows(scopeType, value));
    }
}
