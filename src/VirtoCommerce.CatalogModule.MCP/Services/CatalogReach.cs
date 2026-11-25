using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.MCP.Services;

// The stores a catalog reaches: those that sell from it directly, and those whose catalog links its categories or
// products. A change of the catalog reaches every one of them, whether the user may read them or not, so an answer counts
// them all and names only the ones the user may read: the only figure about objects the user may not read.
public class CatalogReach(IStoreSearchService storeSearchService, IProductSetupService productSetupService, CatalogAccess access)
{
    // Every store under each catalog it sells from, its own catalog included, read once per call: an installation has
    // few stores.
    public virtual async Task<IDictionary<string, IList<Store>>> GetStoresByCatalogAsync()
    {
        var result = new Dictionary<string, IList<Store>>(StringComparer.OrdinalIgnoreCase);
        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        var stores = await storeSearchService.SearchAllNoCloneAsync(criteria);

        foreach (var store in stores.Where(x => !string.IsNullOrEmpty(x.Catalog)))
        {
            var storeCatalogs = await productSetupService.GetStoreCatalogsAsync(store);
            var catalogIds = storeCatalogs.Catalogs.Select(x => x.Id).Append(store.Catalog).Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var catalogId in catalogIds)
            {
                if (!result.TryGetValue(catalogId, out var list))
                {
                    list = [];
                    result[catalogId] = list;
                }

                list.Add(store);
            }
        }

        return result;
    }

    // "B2B-store, Foo and 2 other stores", "2 stores you may not read", or "no store".
    public virtual string Describe(ClaimsPrincipal user, IDictionary<string, IList<Store>> storesByCatalog, string catalogId)
    {
        var stores = storesByCatalog.TryGetValue(catalogId, out var list) ? list : [];
        var names = stores
            .Where(x => access.IsGranted(user, StorePermissions.Read, CatalogAccess.StoreScopeType, x.Id))
            .Select(x => x.Name ?? x.Id)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var others = stores.Count - names.Count;

        if (names.Count == 0)
        {
            return others switch
            {
                0 => "no store",
                1 => "1 store you may not read",
                _ => $"{others} stores you may not read",
            };
        }

        var named = string.Join(", ", names);

        return others switch
        {
            0 => named,
            1 => $"{named} and 1 other store",
            _ => $"{named} and {others} other stores",
        };
    }
}
