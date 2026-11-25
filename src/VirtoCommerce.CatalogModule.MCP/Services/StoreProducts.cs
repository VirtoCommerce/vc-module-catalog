using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ModelContextProtocol;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.Platform.Core.Common;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.MCP.Services;

// The products a user works on in a store, found by SKU or by id, each one checked on its own catalog as the products API
// checks it. A SKU is unique in its physical catalog only, so a store that sells from several catalogs may hold it twice.
public class StoreProducts(CatalogAccess access, IProductSearchService productSearchService, IItemService itemService)
{
    // The user's products of the store with the SKUs, variations included, by SKU as each product holds it: one per catalog
    // that has it. A SKU of a catalog the user may not read is not found.
    public virtual async Task<IList<CatalogProduct>> FindBySkusAsync(ClaimsPrincipal user, CatalogStoreView view, IList<string> skus, string responseGroup)
    {
        var codes = (skus ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        if (codes.Count == 0 || view.ReadableCatalogs.Count == 0)
        {
            return [];
        }

        // The products' own catalogs are the ones the user may read products of (CatalogAccess.GetStoreViewAsync).
        var criteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();
        criteria.CatalogIds = view.ReadableCatalogs.Select(x => x.Id).ToArray();
        criteria.Skus = codes;
        criteria.SearchInVariations = true;
        criteria.ResponseGroup = responseGroup;
        // At most one product per SKU and catalog.
        criteria.Take = codes.Count * view.ReadableCatalogs.Count;

        return (await productSearchService.SearchAsync(criteria)).Results;
    }

    // One product of the store by its SKU, or by its id when two catalogs of the store hold the SKU. Refused when the user
    // may not work on it, with the words of an unknown SKU.
    public virtual async Task<CatalogProduct> GetAsync(ClaimsPrincipal user, CatalogStoreView view, string skuOrId, string responseGroup)
    {
        if (string.IsNullOrWhiteSpace(skuOrId))
        {
            throw new McpException("sku is required. search_products finds products by name.");
        }

        var value = skuOrId.Trim();
        var found = await FindBySkusAsync(user, view, [value], responseGroup);

        if (found.Count > 1)
        {
            var catalogNames = view.ReadableCatalogs.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);
            var holders = found.Select(x => $"{catalogNames.GetValueOrDefault(x.CatalogId, x.CatalogId)} (product id {x.Id})");

            throw new McpException($"SKU '{value}' is in {found.Count} catalogs of store '{view.Store.Id}': {string.Join(", ", holders)}. " +
                "Give the product id instead of the SKU.");
        }

        if (found.Count == 1)
        {
            return found[0];
        }

        var byId = await itemService.GetByIdAsync(value, responseGroup);

        if (byId != null && view.ReadableCatalogs.Any(x => x.Id.EqualsIgnoreCase(byId.CatalogId)))
        {
            return byId;
        }

        throw new McpException(GetUnknownSkuText(view, value));
    }

    // The products of the ids, as the search index lists them, that the user may read, in the order of the ids. Each one is
    // checked on its own catalog: the index lists a product under the catalogs that link it, and a link saved through the
    // API may name a target catalog that its target category is not in, which the walk of the store's catalogs misses.
    public virtual async Task<IList<CatalogProduct>> LoadReadableAsync(ClaimsPrincipal user, IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        var products = await itemService.GetAsync(ids, responseGroup);
        var readable = await access.FilterAllowedAsync(user, products, CatalogPermissions.ProductsRead);
        var order = ids.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index, StringComparer.OrdinalIgnoreCase);

        return readable.OrderBy(x => order.GetValueOrDefault(x.Id, int.MaxValue)).ToList();
    }

    public virtual string GetUnknownSkuText(CatalogStoreView view, string sku)
    {
        return $"SKU '{sku}' is not among the products you may work on in store '{view.Store.Id}'. search_products finds products by name.";
    }
}
