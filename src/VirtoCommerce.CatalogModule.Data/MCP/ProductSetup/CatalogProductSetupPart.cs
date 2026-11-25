using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using CatalogSettings = VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings;

namespace VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;

// Catalog's part of a product as a store sells it: its content, category and flags. Its check mirrors the storefront's
// rules (X-Catalog lists products from the search index with is:product, status:visible, the store's catalog in
// __outline and the date window; X-Catalog and X-Cart sell only an active, buyable product that the index finds). Catalog
// cannot call those modules, as they depend on it: each rule is a protected virtual method, so a solution that changes
// the storefront's rules changes the check the same way.
public class CatalogProductSetupPart(
    IProductSetupFindings setupFindings,
    ProductIndexingTexts indexingTexts,
    Func<ICatalogRepository> repositoryFactory)
    : IProductSetupState, IProductSetupCheck
{
    public const string PartName = "product";

    // The ids of the part's findings.
    public static class FindingIds
    {
        public const string NotInIndex = "product-not-in-index";
        public const string IndexOutdated = "product-index-outdated";
        public const string OutsideDates = "product-outside-dates";
        public const string Inactive = "product-inactive";
        public const string CategoryInactive = "product-category-inactive";
        public const string NotBuyable = "product-not-buyable";
        public const string NotInStoreCatalog = "product-not-in-store-catalog";
        public const string HasVariations = "product-has-variations";
        public const string NoImage = "product-no-image";
        public const string NoDescription = "product-no-description";
        public const string RequiredPropertyMissing = "product-required-property-missing";
    }

    // How far the index's copy of a product may lag behind its last save before it counts as outdated: the providers keep
    // dates at different precisions.
    public static readonly TimeSpan IndexLagTolerance = TimeSpan.FromSeconds(1);

    public virtual string Name => PartName;

    public virtual string Title => "Product";

    public virtual McpSetupPermissions Permissions => ProductSetupPermissions.ForProducts();

    public virtual Task<IDictionary<string, object>> GetStatesAsync(ProductSetupContext context)
    {
        IDictionary<string, object> result = context.Products.ToDictionary(x => x.Id, x => (object)GetState(context, x), StringComparer.OrdinalIgnoreCase);

        return Task.FromResult(result);
    }

    public virtual async Task<IList<McpFinding>> CheckAsync(ProductSetupContext context)
    {
        // Without the index, the rules that matter most cannot be decided: the setup service reports the part as not
        // checked.
        if (context.Index == null)
        {
            throw new InvalidOperationException("The search index could not be read.");
        }

        var findings = new List<McpFinding>();

        if (context.Products.Count == 0)
        {
            return findings;
        }

        var variationCounts = await GetVariationCountsAsync(context.Products);
        var indexingSentence = await indexingTexts.GetIndexingSentenceAsync();
        var now = DateTime.UtcNow;

        foreach (var product in context.Products)
        {
            var productFindings = new[]
            {
                CheckInIndex(context, product, indexingSentence),
                CheckIndexUpToDate(context, product, indexingSentence),
                CheckDates(product, now),
                CheckActive(product),
                CheckCategoriesActive(product),
                CheckBuyable(product),
                CheckInStoreCatalog(context, product),
                CheckVariations(product, variationCounts.TryGetValue(product.Id, out var variationCount) ? variationCount : 0),
                CheckImages(product),
                CheckDescriptions(context, product),
                CheckRequiredProperties(product),
            };

            findings.AddRange(productFindings.Where(x => x != null));
        }

        return findings;
    }

    // The storefront reads products from the search index only: a product that is not there is neither listed nor found
    // by its id, and the cart refuses it.
    protected virtual McpFinding CheckInIndex(ProductSetupContext context, CatalogProduct product, string indexingSentence)
    {
        if (context.Index.TryGetValue(product.Id, out var state) && state.IsIndexed)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.NotInIndex, McpFindingSeverities.Critical);
        finding.Title = "The product is not in the search index";
        finding.Why = "The storefront reads products from the search index: it does not list this product or find it by its id, and the cart refuses it.";
        finding.Evidence = "The search index has no document with the product's id.";
        finding.Fix = $"{indexingSentence} If it stays missing, rebuild the product index on the Search index screen.";

        return finding;
    }

    // The index holds the product as it was before its last change, which the storefront shows until the index updates.
    protected virtual McpFinding CheckIndexUpToDate(ProductSetupContext context, CatalogProduct product, string indexingSentence)
    {
        if (!context.Index.TryGetValue(product.Id, out var state) || !state.IsIndexed || state.ModifiedDate == null || product.ModifiedDate == null)
        {
            return null;
        }

        var saved = ToUtc(product.ModifiedDate.Value);
        var indexed = ToUtc(state.ModifiedDate.Value);

        if (saved - indexed <= IndexLagTolerance)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.IndexOutdated, McpFindingSeverities.Warning);
        finding.Title = "The search index holds an older version of the product";
        finding.Why = "The storefront shows the product as it was before its last change until the index updates.";
        finding.Evidence = $"Saved {FormatDate(saved)}; the index holds the version of {FormatDate(indexed)} (UTC).";
        finding.Fix = $"{indexingSentence} If it stays outdated, rebuild the product index on the Search index screen.";

        return finding;
    }

    // The storefront lists and sells a product only between its start and end dates.
    protected virtual McpFinding CheckDates(CatalogProduct product, DateTime now)
    {
        var start = ToUtc(product.StartDate);
        var end = product.EndDate == null ? (DateTime?)null : ToUtc(product.EndDate.Value);
        string evidence;

        if (start > now)
        {
            evidence = $"Its start date is {FormatDate(start)} (UTC), which has not come yet.";
        }
        else if (end <= now)
        {
            evidence = $"Its end date was {FormatDate(end.Value)} (UTC).";
        }
        else
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.OutsideDates, McpFindingSeverities.Critical);
        finding.Title = "The product is outside its sale dates";
        finding.Why = "The storefront lists and sells a product only between its start and end dates.";
        finding.Evidence = evidence;
        finding.Fix = "Change the product's start or end date in the back office.";

        return finding;
    }

    // An inactive product is hidden: the index marks it hidden, the storefront does not list it and the cart refuses it.
    protected virtual McpFinding CheckActive(CatalogProduct product)
    {
        if (product.IsActive == true)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.Inactive, McpFindingSeverities.Critical);
        finding.Title = "The product is not active";
        finding.Why = "The storefront does not list an inactive product, and the cart refuses it.";
        finding.Evidence = "Active: no.";
        finding.Fix = "Make the product active.";

        return finding;
    }

    // A product under an inactive category is indexed as hidden, so the storefront does not list it.
    protected virtual McpFinding CheckCategoriesActive(CatalogProduct product)
    {
        if (product.ParentCategoryIsActive)
        {
            return null;
        }

        var inactive = GetFirstInactiveCategory(product.Category);
        var finding = CreateFinding(product, FindingIds.CategoryInactive, McpFindingSeverities.Critical);
        finding.Title = "A category above the product is not active";
        finding.Why = "The search index marks the products under an inactive category as hidden, so the storefront does not list them.";
        finding.Evidence = inactive == null ? "A category above the product is inactive." : $"The category '{inactive.Name}' is inactive.";
        finding.Fix = "Make the category active in the back office, or move the product to an active category.";

        return finding;
    }

    // A product that is not buyable is listed, but it cannot be bought.
    protected virtual McpFinding CheckBuyable(CatalogProduct product)
    {
        if (product.IsBuyable == true)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.NotBuyable, McpFindingSeverities.Critical);
        finding.Title = "The product is not buyable";
        finding.Why = "The storefront lists the product, but customers cannot add it to a cart.";
        finding.Evidence = "Buyable: no.";
        finding.Fix = "Make the product buyable.";

        return finding;
    }

    // The storefront shows the products whose outline reaches the store's catalog: its own categories, the categories
    // linked into it and the products linked into it.
    protected virtual McpFinding CheckInStoreCatalog(ProductSetupContext context, CatalogProduct product)
    {
        if (context.StoreCatalog != null && GetStoreOutline(context, product) != null)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.NotInStoreCatalog, McpFindingSeverities.Critical);
        finding.Title = "The product is not in the store's catalog";
        finding.Why = "The storefront lists and finds only the products of the store's catalog, its linked categories and its linked products.";
        finding.Evidence = context.StoreCatalog == null
            ? "The store has no catalog."
            : $"No category of the product reaches the store's catalog '{context.StoreCatalog.Name}'; the product is in '{GetOwnPath(product) ?? product.Catalog?.Name}'.";
        finding.Fix = context.StoreCatalog?.IsVirtual == true
            ? "Link the product, or a category above it, into the store's virtual catalog."
            : "Move the product to a category of the store's catalog, or link it into the store's catalog.";

        return finding;
    }

    // Customers buy the variations of a product that has them.
    protected virtual McpFinding CheckVariations(CatalogProduct product, int variationCount)
    {
        if (variationCount == 0)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.HasVariations, McpFindingSeverities.Info);
        finding.Title = "Customers buy the product's variations";
        finding.Why = "The storefront shows the product with its variations and adds a variation to the cart, not the product itself.";
        finding.Evidence = variationCount == 1 ? "1 variation." : $"{variationCount} variations.";
        finding.Fix = "Check the variations one by one: each is sold on its own.";

        return finding;
    }

    protected virtual McpFinding CheckImages(CatalogProduct product)
    {
        if (!product.Images.IsNullOrEmpty())
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.NoImage, McpFindingSeverities.Warning);
        finding.Title = "The product has no image";
        finding.Why = "The storefront shows a placeholder instead of a picture of the product.";
        finding.Evidence = "Images: none.";
        finding.Fix = "Add an image on the product's screen in the back office.";

        return finding;
    }

    // A description in each of the store's languages: one without a language serves every language.
    protected virtual McpFinding CheckDescriptions(ProductSetupContext context, CatalogProduct product)
    {
        var reviews = (product.Reviews ?? []).Where(x => !string.IsNullOrWhiteSpace(x.Content)).ToList();

        if (reviews.Any(x => string.IsNullOrEmpty(x.LanguageCode)))
        {
            return null;
        }

        var missing = GetStoreLanguages(context)
            .Where(language => !reviews.Any(x => x.LanguageCode.EqualsIgnoreCase(language)))
            .ToList();

        if (missing.Count == 0)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.NoDescription, McpFindingSeverities.Warning);
        finding.Title = missing.Count == 1 ? $"The product has no description in {missing[0]}" : "The product has no description in some of the store's languages";
        finding.Why = "Customers who shop in those languages see no description of the product.";
        finding.Evidence = $"No description in: {string.Join(", ", missing)}.";
        finding.Fix = "Add a description in those languages.";

        return finding;
    }

    // A required property without a value: the save does not refuse it, but the product misses what its category asks for.
    protected virtual McpFinding CheckRequiredProperties(CatalogProduct product)
    {
        var missing = GetOwnProperties(product)
            .Where(x => x.Required && !HasValue(x))
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missing.Count == 0)
        {
            return null;
        }

        var finding = CreateFinding(product, FindingIds.RequiredPropertyMissing, McpFindingSeverities.Warning);
        finding.Title = missing.Count == 1 ? $"The required property '{missing[0]}' has no value" : "Required properties have no value";
        finding.Why = "The product misses information its category requires; the storefront shows the property empty.";
        finding.Evidence = $"No value: {string.Join(", ", missing)}.";
        finding.Fix = "Set the values of these properties.";

        return finding;
    }

    // The product's state, in the shape of Catalog's fields of a product row.
    protected virtual CatalogProductSection GetState(ProductSetupContext context, CatalogProduct product)
    {
        var defaultLanguage = context.Store?.DefaultLanguage;
        var section = AbstractTypeFactory<CatalogProductSection>.TryCreateInstance();
        section.Sku = product.Code;
        section.Name = product.Name;
        section.Category = GetPath(GetStoreOutline(context, product)) ?? GetOwnPath(product);
        section.ParentSku = string.IsNullOrEmpty(product.MainProductId) ? null : product.MainProduct?.Code;
        section.Description = GetDescription(product, defaultLanguage, orWithoutLanguage: true);
        section.Descriptions = GetStoreLanguages(context)
            .Where(x => !x.EqualsIgnoreCase(defaultLanguage))
            .Select(x => (Language: x, Text: GetDescription(product, x, orWithoutLanguage: false)))
            .Where(x => x.Text != null)
            .ToDictionary(x => x.Language, x => x.Text, StringComparer.OrdinalIgnoreCase);
        section.Properties = GetOwnProperties(product)
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => GetValue(x.First(), defaultLanguage), StringComparer.OrdinalIgnoreCase);
        section.Active = product.IsActive;
        section.Buyable = product.IsBuyable;
        section.TrackInventory = product.TrackInventory;
        section.Gtin = product.Gtin;
        section.Vendor = product.Vendor;
        section.ManufacturerPartNumber = product.ManufacturerPartNumber;

        return section;
    }

    // The properties a product row sets: a product's own ones, and a variation's own and variation ones.
    protected virtual IEnumerable<Property> GetOwnProperties(CatalogProduct product)
    {
        var isVariation = !string.IsNullOrEmpty(product.MainProductId);

        return (product.Properties ?? []).Where(x => x.Type == PropertyType.Product || (isVariation && x.Type == PropertyType.Variation));
    }

    // A property's value as a row takes it: a list for a property with several values, a dictionary value by its alias,
    // a property in several languages in the store's default language.
    protected virtual object GetValue(Property property, string language)
    {
        var values = (property.Values ?? []).Where(x => x.Value != null || !string.IsNullOrEmpty(x.Alias)).ToList();

        if (property.Multilanguage && values.Count > 0)
        {
            var inLanguage = values.Where(x => x.LanguageCode.EqualsIgnoreCase(language)).ToList();
            values = inLanguage.Count > 0 ? inLanguage : values.Where(x => x.LanguageCode.EqualsIgnoreCase(values[0].LanguageCode)).ToList();
        }

        var items = values.Select(x => property.Dictionary && !string.IsNullOrEmpty(x.Alias) ? x.Alias : x.Value).ToList();

        return property.Multivalue ? items : items.FirstOrDefault();
    }

    protected virtual bool HasValue(Property property)
    {
        return (property.Values ?? []).Any(x => !string.IsNullOrEmpty(x.ValueId) || !string.IsNullOrWhiteSpace(x.Value?.ToString()));
    }

    // The description that a product row's description fields set: the default review type, in the language. The
    // store's default language also shows a description saved without a language, which the storefront shows in every
    // language.
    protected virtual string GetDescription(CatalogProduct product, string language, bool orWithoutLanguage)
    {
        var reviewType = CatalogSettings.General.EditorialReviewTypes.DefaultValue?.ToString();
        var reviews = (product.Reviews ?? [])
            .Where(x => !string.IsNullOrEmpty(x.Content) && (string.IsNullOrEmpty(x.ReviewType) || x.ReviewType.EqualsIgnoreCase(reviewType)))
            .ToList();

        return reviews.FirstOrDefault(x => x.LanguageCode.EqualsIgnoreCase(language))?.Content
            ?? (orWithoutLanguage ? reviews.FirstOrDefault(x => string.IsNullOrEmpty(x.LanguageCode))?.Content : null);
    }

    protected virtual IList<string> GetStoreLanguages(ProductSetupContext context)
    {
        return (context.Store?.Languages ?? [])
            .Prepend(context.Store?.DefaultLanguage)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // The product's outline in the store's catalog, as customers browse to it.
    protected virtual Outline GetStoreOutline(ProductSetupContext context, CatalogProduct product)
    {
        var storeCatalogId = context.StoreCatalog?.Id;

        return storeCatalogId == null ? null : (product.Outlines ?? []).FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(storeCatalogId) == true);
    }

    // The product's category path in its own catalog.
    protected virtual string GetOwnPath(CatalogProduct product)
    {
        return GetPath((product.Outlines ?? []).FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(product.CatalogId) == true));
    }

    // "Printers > Laser": the categories between the outline's catalog and the product; null for a product at a catalog's
    // root or without the outline.
    protected virtual string GetPath(Outline outline)
    {
        if (outline?.Items == null || outline.Items.Count < 3)
        {
            return null;
        }

        return string.Join(" > ", outline.Items.Skip(1).Take(outline.Items.Count - 2).Select(x => x.Name));
    }

    protected virtual Category GetFirstInactiveCategory(Category category)
    {
        for (var current = category; current != null; current = current.Parent)
        {
            if (current.IsActive != true)
            {
                return current;
            }
        }

        return null;
    }

    // The number of variations of each product, in one query.
    protected virtual async Task<IDictionary<string, int>> GetVariationCountsAsync(IList<CatalogProduct> products)
    {
        var ids = products.Where(x => string.IsNullOrEmpty(x.MainProductId)).Select(x => x.Id).ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        using var repository = repositoryFactory();
        repository.DisableChangesTracking();

        var counts = await repository.Items
            .Where(x => ids.Contains(x.ParentId))
            .GroupBy(x => x.ParentId)
            .Select(x => new { Id = x.Key, Count = x.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.Id, x => x.Count, StringComparer.OrdinalIgnoreCase);
    }

    protected virtual McpFinding CreateFinding(CatalogProduct product, string id, string severity)
    {
        return setupFindings.ForProduct(product, Name, id, severity);
    }

    protected static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    protected static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
