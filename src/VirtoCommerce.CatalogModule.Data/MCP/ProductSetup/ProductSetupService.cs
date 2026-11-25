using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;

public class ProductSetupService(
    IEnumerable<IProductSetupPart> parts,
    IItemService itemService,
    ICatalogService catalogService,
    Func<ICatalogRepository> repositoryFactory,
    ISearchProvider searchProvider,
    IProductSetupFindings setupFindings,
    ILogger<ProductSetupService> logger)
    : IProductSetupService
{
    // What the parts read of a product: the storefront's view of it, without variations, associations and inventory.
    public const ItemResponseGroup ProductResponseGroup = ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemAssets |
        ItemResponseGroup.ItemProperties | ItemResponseGroup.ItemEditorialReviews | ItemResponseGroup.Links | ItemResponseGroup.Outlines;

    // Catalog's part comes first: the other parts read the products it plans. The other parts do not depend on each other,
    // so they follow by name, in an order no module has to choose.
    public virtual IList<IProductSetupPart> GetParts()
    {
        return parts
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Last())
            .OrderBy(x => x.Name.EqualsIgnoreCase(CatalogProductSetupPart.PartName) ? 0 : 1)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public virtual async Task<StoreCatalogs> GetStoreCatalogsAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var result = AbstractTypeFactory<StoreCatalogs>.TryCreateInstance();
        var storeCatalog = string.IsNullOrEmpty(store.Catalog) ? null : await catalogService.GetByIdAsync(store.Catalog);

        if (storeCatalog == null)
        {
            return result;
        }

        result.StoreCatalog = storeCatalog;
        var catalogIds = await GetSoldCatalogIdsAsync(storeCatalog);
        var catalogs = catalogIds.Count == 0 ? [] : await catalogService.GetAsync(catalogIds.ToList());
        result.Catalogs = catalogs.Where(x => !x.IsVirtual).OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var catalog in result.Catalogs)
        {
            result.LinkedFrom[catalog.Id] = await GetLinkingCatalogIdsAsync(catalog.Id);
        }

        return result;
    }

    public virtual async Task<IDictionary<string, IDictionary<string, object>>> GetStatesAsync(Store store, IList<string> productIds)
    {
        var context = await CreateContextAsync(store, productIds);
        var result = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in GetParts().OfType<IProductSetupState>())
        {
            try
            {
                result[part.Name] = await part.GetStatesAsync(context) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                // The answer never carries an exception's text: it may hold a third party's trace or a secret. The part's
                // check reports the part as not checked, and the tools show its state as unavailable.
                logger.LogError(ex, "The product setup part {Part} could not read the products of the store {StoreId}", part.Name, store.Id);
            }
        }

        return result;
    }

    public virtual async Task<IList<McpFinding>> CheckAsync(Store store, IList<string> productIds)
    {
        var context = await CreateContextAsync(store, productIds);

        return await RunChecksAsync(context);
    }

    // A critical finding blocks the sale, whatever the case of its severity.
    public virtual bool BlocksSale(McpFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);

        return McpFindingSeverities.Critical.EqualsIgnoreCase(finding.Severity);
    }

    protected virtual async Task<ProductSetupContext> CreateContextAsync(Store store, IList<string> productIds)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(productIds);

        var storeCatalogs = await GetStoreCatalogsAsync(store);
        var context = AbstractTypeFactory<ProductSetupContext>.TryCreateInstance();
        context.Store = store.CloneTyped();
        context.StoreCatalog = storeCatalogs.StoreCatalog;
        context.Catalogs = storeCatalogs.Catalogs;

        var ids = productIds.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        // Copies, which a part may change in place.
        context.Products = ids.Count == 0 ? [] : await itemService.GetAsync(ids, ProductResponseGroup.ToString());
        context.Index = await GetIndexStatesAsync(context);

        return context;
    }

    protected virtual async Task<IList<McpFinding>> RunChecksAsync(ProductSetupContext context)
    {
        var findings = new List<McpFinding>();

        foreach (var check in GetParts().OfType<IProductSetupCheck>())
        {
            try
            {
                // A finding belongs to the part that found it, whatever category it names: the part's read permission
                // decides who sees it.
                foreach (var finding in await check.CheckAsync(context) ?? [])
                {
                    finding.Category = check.Name;
                    findings.Add(finding);
                }
            }
            catch (Exception ex)
            {
                // The answer never carries an exception's text: it may hold a third party's trace or a secret.
                logger.LogError(ex, "The product setup part {Part} could not check the products of the store {StoreId}", check.Name, context.Store.Id);
                findings.AddRange(context.Products.Select(x => CreateNotCheckedFinding(check, x)));
            }
        }

        return findings;
    }

    // What the search index holds for the products, in one query by their ids: whether each one is there, its change date
    // and the store's currencies it has a price in. Null when the index cannot be read: the product part then reports
    // itself as not checked.
    protected virtual async Task<IDictionary<string, ProductIndexState>> GetIndexStatesAsync(ProductSetupContext context)
    {
        var result = new Dictionary<string, ProductIndexState>(StringComparer.OrdinalIgnoreCase);

        if (context.Products.Count == 0)
        {
            return result;
        }

        var currencies = (context.Store.Currencies ?? []).Append(context.Store.DefaultCurrency)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var request = AbstractTypeFactory<SearchRequest>.TryCreateInstance();
        request.Filter = new IdsFilter { Values = context.Products.Select(x => x.Id).ToList() };
        request.Skip = 0;
        request.Take = context.Products.Count;
        request.IncludeFields = currencies.Select(GetPriceField).Prepend(IndexFields.ModifiedDate).ToList();

        SearchResponse response;

        try
        {
            response = await searchProvider.SearchAsync(KnownDocumentTypes.Product, request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The search index could not be read for the products of the store {StoreId}", context.Store.Id);
            return null;
        }

        foreach (var document in response?.Documents ?? [])
        {
            var state = AbstractTypeFactory<ProductIndexState>.TryCreateInstance();
            state.IsIndexed = true;
            state.ModifiedDate = GetDate(document, IndexFields.ModifiedDate);
            state.PriceCurrencies = currencies.Where(x => HasValue(document, GetPriceField(x))).ToList();
            result[document.Id] = state;
        }

        foreach (var product in context.Products.Where(x => !result.ContainsKey(x.Id)))
        {
            result[product.Id] = AbstractTypeFactory<ProductIndexState>.TryCreateInstance();
        }

        return result;
    }

    // The ids of the catalogs whose products the store's catalog holds: its own when it is physical, and every catalog that
    // links categories or products into it, or into the subtree of a category linked into it, at any depth. The walk keeps
    // the categories it has seen, so links that close a loop end it. Its cost grows with the linked categories and the
    // links, never with the products.
    protected virtual async Task<ISet<string>> GetSoldCatalogIdsAsync(Catalog storeCatalog)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!storeCatalog.IsVirtual)
        {
            result.Add(storeCatalog.Id);
        }

        using var repository = repositoryFactory();
        repository.DisableChangesTracking();

        var seenCategoryIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var linkedCategories = await IntoCatalog(repository.CategoryLinks, storeCatalog.Id)
            .Select(x => new { x.SourceCategoryId, x.SourceCategory.CatalogId })
            .ToListAsync();
        var productCatalogIds = await IntoCatalog(repository.CategoryItemRelations, storeCatalog.Id)
            .Select(x => x.CatalogItem.CatalogId)
            .Distinct()
            .ToListAsync();
        result.UnionWith(productCatalogIds);

        var pending = linkedCategories.Where(x => seenCategoryIds.Add(x.SourceCategoryId)).ToList();

        while (pending.Count > 0)
        {
            result.UnionWith(pending.Select(x => x.CatalogId));

            var categoryIds = pending.Select(x => x.SourceCategoryId).ToList();
            var subtreeIds = categoryIds.Concat(await repository.GetAllChildrenCategoriesIdsAsync(categoryIds)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            seenCategoryIds.UnionWith(subtreeIds);

            linkedCategories = await repository.CategoryLinks
                .Where(x => subtreeIds.Contains(x.TargetCategoryId))
                .Select(x => new { x.SourceCategoryId, x.SourceCategory.CatalogId })
                .ToListAsync();
            productCatalogIds = await repository.CategoryItemRelations
                .Where(x => subtreeIds.Contains(x.CategoryId))
                .Select(x => x.CatalogItem.CatalogId)
                .Distinct()
                .ToListAsync();
            result.UnionWith(productCatalogIds);

            pending = linkedCategories.Where(x => seenCategoryIds.Add(x.SourceCategoryId)).ToList();
        }

        return result;
    }

    // The ids of the catalogs that link categories or products into the catalog, anywhere in it.
    protected virtual async Task<IList<string>> GetLinkingCatalogIdsAsync(string catalogId)
    {
        using var repository = repositoryFactory();
        repository.DisableChangesTracking();

        var fromCategories = await IntoCatalog(repository.CategoryLinks, catalogId)
            .Select(x => x.SourceCategory.CatalogId)
            .Distinct()
            .ToListAsync();
        var fromProducts = await IntoCatalog(repository.CategoryItemRelations, catalogId)
            .Select(x => x.CatalogItem.CatalogId)
            .Distinct()
            .ToListAsync();

        return fromCategories.Concat(fromProducts)
            .Where(x => !x.EqualsIgnoreCase(catalogId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // The category links into a catalog as outlines follow them: a link into a category belongs to the category's catalog,
    // whatever target catalog the link names (the API saves any), and a link into a catalog's root to that catalog.
    protected virtual IQueryable<CategoryRelationEntity> IntoCatalog(IQueryable<CategoryRelationEntity> links, string catalogId)
    {
        return links.Where(x => (x.TargetCategoryId == null && x.TargetCatalogId == catalogId) ||
            (x.TargetCategoryId != null && x.TargetCategory.CatalogId == catalogId));
    }

    // The product links into a catalog, as the category links.
    protected virtual IQueryable<CategoryItemRelationEntity> IntoCatalog(IQueryable<CategoryItemRelationEntity> links, string catalogId)
    {
        return links.Where(x => (x.CategoryId == null && x.CatalogId == catalogId) || (x.CategoryId != null && x.Category.CatalogId == catalogId));
    }

    protected virtual McpFinding CreateNotCheckedFinding(IProductSetupPart part, CatalogProduct product)
    {
        var finding = setupFindings.ForProduct(product, part.Name, $"{part.Name}-not-checked", McpFindingSeverities.Critical);
        finding.Title = $"{part.Title} could not be checked";
        finding.Why = "A product is counted as one that can be bought only when every part of it is checked.";
        finding.Evidence = "The check failed; the platform log has the details.";
        finding.Fix = "Review the product in the back office, or check again later.";

        return finding;
    }

    // The field of the product's price in a currency, as Pricing indexes it: "price_usd".
    protected virtual string GetPriceField(string currency)
    {
        return $"{IndexFields.PricePrefix}{currency}".ToLowerInvariant();
    }

    protected virtual DateTime? GetDate(SearchDocument document, string field)
    {
        if (!document.TryGetValue(field, out var value) || value == null)
        {
            return null;
        }

        var single = value is object[] values ? values.FirstOrDefault() : value;

        return single switch
        {
            DateTime date => date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime(),
            DateTimeOffset date => date.UtcDateTime,
            string text when DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var date) => date,
            _ => null,
        };
    }

    protected virtual bool HasValue(SearchDocument document, string field)
    {
        return document.TryGetValue(field, out var value) && value != null && (value is not object[] values || values.Length > 0);
    }

    public static class IndexFields
    {
        public const string ModifiedDate = "modifieddate";
        public const string PricePrefix = "price_";
    }
}
