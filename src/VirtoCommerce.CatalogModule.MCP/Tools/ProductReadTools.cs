using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.CatalogModule.MCP.Services;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.MCP.Tools;

// A store's products as its customers get them: found in the search index, which the storefront reads, and checked
// exactly by every part (content, price, stock). A product tool takes the store as the context of the price, the stock and
// whether the product can be bought; the user works on a product when its own catalog allows it.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(CatalogPermissions.Read, StorePermissions.Read)]
public class ProductReadTools(
    CatalogAccess access,
    CatalogTree tree,
    StoreProductIndex productIndex,
    StoreProducts storeProducts,
    ProductFindingRows findingRows,
    CatalogBackOfficeLinks links,
    IProductSetupService setupService,
    IProductSearchService productSearchService,
    IMcpPageSizeResolver pageSizeResolver,
    ILogger<ProductReadTools> logger)
{
    // The most SKUs one call checks.
    public const int MaxSkus = 100;

    // The most variations get_product lists.
    public const int MaxVariations = 20;

    // What a row of a list needs of a product: its outlines give its category path in the store.
    public const string RowResponseGroup = $"{nameof(ItemResponseGroup.ItemInfo)},{nameof(ItemResponseGroup.Outlines)}";

    public const string OtherCatalogsNote = "This store also sells products of catalogs you may not read: they are not shown or counted.";

    public const string NotExactNote = "Products of these catalogs that are also linked into catalogs you may not read are left out of " +
        "the search index's lists and figures; find_products_not_for_sale with skus checks them.";

    [McpServerTool(Name = "search_products", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Finds a store's products in the search index, which the storefront reads, by keyword or category, a page at " +
        "a time: SKU, name, category path, whether customers can buy it and the first reason when not. Lists the products of " +
        "the catalogs you may read. get_product shows one product in full.")]
    public async Task<object> SearchProducts(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as get_catalog_overview lists it.")] string store_id,
        [Description("Words to find in the products' names, SKUs, descriptions and property values.")] string keyword = null,
        [Description("A category of the store, by its path as customers browse it, such as 'Printers > Laser', or by its id.")] string category = null,
        [Description("Products to skip, for the next page.")] int? skip = null,
        [Description("Products per page.")] int? take = null)
    {
        var user = context.User;
        var view = await access.GetStoreViewAsync(user, store_id);
        var paging = await pageSizeResolver.GetPagingAsync(skip, take);
        var node = await tree.FindAsync(user, view.StoreCatalog, category, nameof(CategoryResponseGroup.Info));
        IList<SortingField> sorting = string.IsNullOrWhiteSpace(keyword) ? [new SortingField(StoreProductIndex.NameSortField)] : null;
        var (ids, totalCount) = await productIndex.SearchAsync(view, keyword, node?.Outline, sorting, paging.Skip, paging.Take);
        var products = await storeProducts.LoadReadableAsync(user, ids, RowResponseGroup);
        var findings = await CheckAsync(view, products);
        var link = await links.GetCatalogLinkAsync();
        var rows = new List<object>();

        foreach (var product in products)
        {
            rows.Add(await GetRowAsync(user, view, product, findings, link));
        }

        return new
        {
            store_id = view.Store.Id,
            products = McpPage<object>.Create(rows, (int)Math.Min(totalCount, int.MaxValue), paging),
            other_catalogs = view.HasOtherCatalogs ? OtherCatalogsNote : null,
            not_exact = view.IndexFiguresExact ? null : NotExactNote,
            next_step = "get_product shows a product with what keeps it from sale; find_products_not_for_sale checks the store's products " +
                "a window at a time.",
        };
    }

    [McpServerTool(Name = "get_product", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("One product of a store in the shape the product tools take: its content, category and property values, and " +
        "each installed part's section, such as its prices and stock in the store; then its variations, whether it is in the " +
        "search index, whether customers can buy it, and the findings that say why not and how to fix it.")]
    public async Task<object> GetProduct(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as get_catalog_overview lists it.")] string store_id,
        [Description("SKU of the product, or its id when an answer says that two catalogs of the store hold the SKU.")] string sku)
    {
        var user = context.User;
        var view = await access.GetStoreViewAsync(user, store_id);
        var product = await storeProducts.GetAsync(user, view, sku, RowResponseGroup);
        var (variations, variationCount) = await GetVariationsAsync(user, product);
        var findings = await CheckAsync(view, [product, .. variations]);
        var link = await links.GetCatalogLinkAsync();
        var (rows, notShown) = await findingRows.CreateAsync(user, product, findings, link);
        var variationRows = new List<object>();

        foreach (var variation in variations)
        {
            variationRows.Add(await GetRowAsync(user, view, variation, findings, link));
        }

        return new
        {
            store_id = view.Store.Id,
            product_id = product.Id,
            catalog = view.ReadableCatalogs.Where(x => x.Id.EqualsIgnoreCase(product.CatalogId)).Select(x => new { id = x.Id, name = x.Name }).FirstOrDefault(),
            product = await GetProductRowAsync(user, view, product),
            in_search_index = IsInIndex(product, findings),
            can_be_bought = findingRows.CanBeBought(product, findings),
            findings = rows,
            not_shown = notShown,
            variations = variationRows.Count > 0 ? variationRows : null,
            variations_count = variationCount > 0 ? variationCount : (int?)null,
            checked_parts = GetCheckedParts(),
            back_office = link,
            next_step = findingRows.CanBeBought(product, findings)
                ? "Customers can buy the product in this store."
                : "Fix the findings that block the sale, then check again with get_product.",
        };
    }

    [McpServerTool(Name = "find_products_not_for_sale", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("Which products of a store customers cannot buy, and why, checked exactly by every part (content, search index, " +
        "price, stock). With skus: those products, also ones not in the search index yet; otherwise one window of the store's " +
        "products, most recently changed first, and next_skip for the next window. Also the store-wide counts of hidden products " +
        "and of products outside their dates.")]
    public async Task<object> FindProductsNotForSale(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as get_catalog_overview lists it.")] string store_id,
        [Description("SKUs to check, at most 100; left out, a window of the store's products.")] string[] skus = null,
        [Description("A category of the store, by its path as customers browse it or by its id, for a window of its products.")] string category = null,
        [Description("Products to skip: the next_skip of the previous window.")] int? skip = null,
        [Description("Products per window.")] int? take = null)
    {
        var user = context.User;
        var view = await access.GetStoreViewAsync(user, store_id);

        if (skus?.Length > MaxSkus)
        {
            throw new McpException($"{skus.Length} SKUs were sent; one call checks at most {MaxSkus}.");
        }

        IList<CatalogProduct> products;
        IList<string> notFound = null;
        string window;
        int? nextSkip = null;

        if (skus?.Length > 0)
        {
            if (!string.IsNullOrWhiteSpace(category) || skip != null || take != null)
            {
                throw new McpException("Send either skus, or a window with category, skip and take: not both.");
            }

            products = await storeProducts.FindBySkusAsync(user, view, skus, RowResponseGroup);
            notFound = skus.Where(sku => !string.IsNullOrWhiteSpace(sku) && !products.Any(x => x.Code.EqualsIgnoreCase(sku.Trim())))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            window = skus.Length == 1 ? "The SKU you named." : $"The {skus.Length} SKUs you named.";
        }
        else
        {
            var paging = await pageSizeResolver.GetPagingAsync(skip, take);
            var node = await tree.FindAsync(user, view.StoreCatalog, category, nameof(CategoryResponseGroup.Info));
            var (ids, totalCount) = await productIndex.SearchAsync(view, keyword: null, node?.Outline,
                [new SortingField(StoreProductIndex.ModifiedDateField, isDescending: true)], paging.Skip, paging.Take);
            products = await storeProducts.LoadReadableAsync(user, ids, RowResponseGroup);
            window = GetWindowText(paging.Skip, ids.Count, totalCount, node?.Path);
            nextSkip = paging.Skip + ids.Count < totalCount && ids.Count > 0 ? paging.Skip + ids.Count : null;
        }

        var findings = await CheckAsync(view, products);
        var link = await links.GetCatalogLinkAsync();
        var notForSale = new List<object>();

        foreach (var product in products.Where(x => !findingRows.CanBeBought(x, findings)))
        {
            notForSale.Add(await GetNotForSaleRowAsync(user, view, product, findings, link));
        }

        return new
        {
            store_id = view.Store.Id,
            @checked = window,
            not_for_sale = notForSale,
            for_sale = products.Count - notForSale.Count,
            not_found = notFound?.Count > 0
                ? notFound.Select(x => storeProducts.GetUnknownSkuText(view, x)).ToList()
                : null,
            next_skip = nextSkip,
            store_wide = await GetStoreWideCountsAsync(view),
            other_catalogs = view.HasOtherCatalogs ? OtherCatalogsNote : null,
            not_in_index = skus?.Length > 0 ? null : "A product that is not in the search index yet is found by its SKU only: send it in skus.",
            checked_parts = GetCheckedParts(),
            next_step = nextSkip != null
                ? "Call again with skip = next_skip for the next window."
                : "Fix the findings that block the sale; get_product shows a product with all its findings.",
        };
    }

    protected virtual async Task<IList<McpFinding>> CheckAsync(CatalogStoreView view, IList<CatalogProduct> products)
    {
        return products.Count == 0 ? [] : await setupService.CheckAsync(view.Store, products.Select(x => x.Id).ToList());
    }

    // A product of a list: what it is, where customers find it, and whether they can buy it.
    protected virtual async Task<object> GetRowAsync(ClaimsPrincipal user, CatalogStoreView view, CatalogProduct product, IList<McpFinding> findings, string link)
    {
        var canBeBought = findingRows.CanBeBought(product, findings);
        string reason = null;

        if (!canBeBought)
        {
            var (rows, notShown) = await findingRows.CreateAsync(user, product, findings, link);
            reason = rows.FirstOrDefault(x => x.BlocksSale)?.Title ?? notShown.FirstOrDefault();
        }

        return new
        {
            sku = product.Code,
            name = product.Name,
            category = GetStorePath(view, product),
            can_be_bought = canBeBought,
            reason,
        };
    }

    protected virtual async Task<object> GetNotForSaleRowAsync(ClaimsPrincipal user, CatalogStoreView view, CatalogProduct product, IList<McpFinding> findings, string link)
    {
        var (rows, notShown) = await findingRows.CreateAsync(user, product, findings, link);

        return new
        {
            sku = product.Code,
            name = product.Name,
            category = GetStorePath(view, product),
            reasons = rows.Where(x => x.BlocksSale).Select(x => new { x.Id, x.Title, x.Fix }).ToList(),
            warnings = rows.Where(x => !x.BlocksSale && x.Severity.EqualsIgnoreCase(McpFindingSeverities.Warning)).Select(x => x.Title).ToList() is { Count: > 0 } warnings ? warnings : null,
            not_shown = notShown.Count > 0 ? notShown : null,
        };
    }

    // The product in the shape of a product row: Catalog's fields, then each other part's section under its name, or why
    // it is not shown.
    protected virtual async Task<JsonObject> GetProductRowAsync(ClaimsPrincipal user, CatalogStoreView view, CatalogProduct product)
    {
        var states = await setupService.GetStatesAsync(view.Store, [product.Id]);
        var row = new JsonObject();

        if (states.TryGetValue(CatalogProductSetupPart.PartName, out var catalogStates) && catalogStates.TryGetValue(product.Id, out var catalogState))
        {
            foreach (var (key, value) in JsonSerializer.SerializeToNode(catalogState, McpSerialization.Options)?.AsObject() ?? [])
            {
                row[key] = value?.DeepClone();
            }
        }

        foreach (var part in setupService.GetParts().OfType<IProductSetupState>().Where(x => !x.Name.EqualsIgnoreCase(CatalogProductSetupPart.PartName)))
        {
            if (!await access.CanReadPartAsync(user, part, product))
            {
                row[part.Name] = $"not shown ({access.GetReadRequirement(part)})";
            }
            else if (!states.TryGetValue(part.Name, out var partStates))
            {
                logger.LogWarning("The product setup part {Part} gave no state for the product {ProductId}", part.Name, product.Id);
                row[part.Name] = $"{part.Title} could not be read; the platform log has the details.";
            }
            else
            {
                row[part.Name] = partStates.TryGetValue(product.Id, out var state) ? JsonSerializer.SerializeToNode(state, McpSerialization.Options) : null;
            }
        }

        return row;
    }

    // A product's variations, at most MaxVariations of them, that the user may read, and how many it has.
    protected virtual async Task<(IList<CatalogProduct> Variations, int TotalCount)> GetVariationsAsync(ClaimsPrincipal user, CatalogProduct product)
    {
        if (!string.IsNullOrEmpty(product.MainProductId))
        {
            return ([], 0);
        }

        var criteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();
        criteria.MainProductId = product.Id;
        criteria.ResponseGroup = RowResponseGroup;
        criteria.Take = MaxVariations;

        var result = await productSearchService.SearchAsync(criteria);
        var variations = await access.FilterAllowedAsync(user, result.Results, CatalogPermissions.ProductsRead);

        return (variations, result.TotalCount);
    }

    // Whether the index has the product; null when the product part could not check it.
    protected virtual bool? IsInIndex(CatalogProduct product, IList<McpFinding> findings)
    {
        var own = findings.Where(x => x.ObjectId.EqualsIgnoreCase(product.Id) && x.Category.EqualsIgnoreCase(CatalogProductSetupPart.PartName)).ToList();

        if (own.Any(x => x.Id.EqualsIgnoreCase($"{CatalogProductSetupPart.PartName}-not-checked")))
        {
            return null;
        }

        return !own.Any(x => x.Id.EqualsIgnoreCase(CatalogProductSetupPart.FindingIds.NotInIndex));
    }

    // The store-wide counts of the reasons the search index knows for every product at once.
    protected virtual async Task<object> GetStoreWideCountsAsync(CatalogStoreView view)
    {
        if (!view.IndexFiguresExact)
        {
            return new { not_counted = NotExactNote };
        }

        try
        {
            return new
            {
                hidden = await productIndex.CountAsync(view, productIndex.GetHiddenFilter()),
                outside_dates = await productIndex.CountAsync(view, productIndex.GetOutsideDatesFilter(DateTime.UtcNow)),
            };
        }
        catch (Exception ex) when (ex is not McpException)
        {
            // The answer never carries an exception's text: it may hold a third party's trace or a secret.
            logger.LogError(ex, "The search index could not be read for the store {StoreId}", view.Store.Id);

            return new { not_counted = "The search index could not be read; the platform log has the details." };
        }
    }

    protected virtual IList<string> GetCheckedParts()
    {
        return setupService.GetParts().OfType<IProductSetupCheck>().Select(x => x.Name).ToList();
    }

    // "Products 1 to 50 of 1,204,311 in 'Printers', most recently changed first."
    protected virtual string GetWindowText(int skip, int count, long totalCount, string categoryPath)
    {
        var where = string.IsNullOrEmpty(categoryPath) ? "of the store" : $"in '{categoryPath}'";

        return count == 0
            ? $"No products {where} from {skip + 1} on; the search index lists {totalCount:N0}."
            : $"Products {skip + 1:N0} to {skip + count:N0} of {totalCount:N0} {where}, most recently changed first.";
    }

    // The product's category path as the store's customers browse to it, or in its own catalog when the store does not
    // reach it.
    protected virtual string GetStorePath(CatalogStoreView view, CatalogProduct product)
    {
        var outline = (product.Outlines ?? []).FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(view.StoreCatalog.Id) == true)
            ?? (product.Outlines ?? []).FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(product.CatalogId) == true);

        return outline?.Items == null || outline.Items.Count < 3
            ? null
            : string.Join(CatalogTree.PathSeparator, outline.Items.Skip(1).Take(outline.Items.Count - 2).Select(x => x.Name));
    }
}
