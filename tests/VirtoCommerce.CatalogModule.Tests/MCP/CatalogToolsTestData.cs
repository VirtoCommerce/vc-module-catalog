using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

// The data of the access matrix: the physical catalog X, the physical catalog Y, whose every name, SKU and id carries the
// marker LEAK-Y, and the virtual catalog V, which links a category of each; the store X sells from X, the store Y from Y,
// the store V from V. Outlines and index documents are derived from it as the platform builds them.
public class CatalogToolsTestData
{
    public const string Marker = "LEAK-Y";

    public const string CatalogX = "catalog-x";
    public const string CatalogY = "LEAK-Y-catalog";
    public const string CatalogV = "catalog-v";

    public const string StoreX = "store-x";
    public const string StoreY = "store-y";
    public const string StoreV = "store-v";

    public const string PrintersX = "x-printers";
    public const string LaserX = "x-laser";
    public const string ToolsY = "LEAK-Y-tools";
    public const string OfficeV = "v-office";

    public const string LaserPrinter = "x-p1";
    public const string OldPrinter = "x-p2";
    public const string Drill = "LEAK-Y-p1";

    public static readonly DateTime Saved = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

    public List<Catalog> Catalogs { get; } =
    [
        new() { Id = CatalogX, Name = "Office X", Languages = [new CatalogLanguage { LanguageCode = "en-US", IsDefault = true }] },
        new() { Id = CatalogY, Name = "LEAK-Y Catalog", Languages = [new CatalogLanguage { LanguageCode = "en-US", IsDefault = true }] },
        new() { Id = CatalogV, Name = "Virtual V", IsVirtual = true, Languages = [new CatalogLanguage { LanguageCode = "en-US", IsDefault = true }] },
    ];

    public List<Category> Categories { get; } =
    [
        new() { Id = PrintersX, Name = "Printers", CatalogId = CatalogX, IsActive = true },
        new() { Id = LaserX, Name = "Laser", CatalogId = CatalogX, ParentId = PrintersX, IsActive = true },
        new() { Id = ToolsY, Name = "LEAK-Y Tools", CatalogId = CatalogY, IsActive = true },
        new() { Id = OfficeV, Name = "Office", CatalogId = CatalogV, IsActive = true },
    ];

    // Category links: the source category (ListEntryId) shown inside the target catalog or category.
    public List<CategoryLink> CategoryLinks { get; } =
    [
        new() { ListEntryId = PrintersX, ListEntryType = nameof(Category), CatalogId = CatalogV, CategoryId = OfficeV },
        new() { ListEntryId = ToolsY, ListEntryType = nameof(Category), CatalogId = CatalogV, CategoryId = OfficeV },
    ];

    public List<CatalogProduct> Products { get; } =
    [
        CreateProduct(LaserPrinter, "X-100", "Laser printer", CatalogX, LaserX),
        CreateProduct(OldPrinter, "X-200", "Old printer", CatalogX, PrintersX, isActive: false),
        CreateProduct(Drill, "LEAK-Y-100", "LEAK-Y Drill", CatalogY, ToolsY),
    ];

    public List<Store> Stores { get; } =
    [
        CreateStore(StoreX, "Store X", CatalogX),
        CreateStore(StoreY, "Store Y", CatalogY),
        CreateStore(StoreV, "Store V", CatalogV),
    ];

    // Products the search index has no document of.
    public HashSet<string> NotIndexed { get; } = new(StringComparer.OrdinalIgnoreCase);

    // The documents the index holds where they differ from the products as saved: an index that has not caught up yet.
    public Dictionary<string, Dictionary<string, List<object>>> IndexedDocuments { get; } = new(StringComparer.OrdinalIgnoreCase);

    // The change dates the index holds, where they differ from the products' own.
    public Dictionary<string, DateTime> IndexedModifiedDates { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Catalog GetCatalog(string id)
    {
        return Catalogs.First(x => x.Id == id);
    }

    public Category GetCategory(string id)
    {
        return Categories.First(x => x.Id == id);
    }

    public CatalogProduct GetProduct(string id)
    {
        return Products.First(x => x.Id == id);
    }

    // A category's outlines: in its own catalog, and inside every catalog that links it or a category above it.
    public IList<Outline> GetOutlines(Category category)
    {
        var own = GetOwnItems(category);
        var outlines = new List<Outline> { new() { Items = own } };

        for (var i = 1; i < own.Count; i++)
        {
            foreach (var link in CategoryLinks.Where(x => x.ListEntryId == own[i].Id))
            {
                var target = link.CategoryId == null ? [CatalogItem(GetCatalog(link.CatalogId))] : GetOwnItems(GetCategory(link.CategoryId));
                outlines.Add(new Outline { Items = target.Concat(own.Skip(i)).ToList() });
            }
        }

        return outlines;
    }

    // A product's outlines: each outline of its category, followed by the product.
    public IList<Outline> GetOutlines(CatalogProduct product)
    {
        return GetOutlines(GetCategory(product.CategoryId))
            .Select(x => new Outline { Items = x.Items.Append(new OutlineItem { Id = product.Id, Name = product.Name }).ToList() })
            .ToList();
    }

    // The fields of a product's index document, as ProductDocumentBuilder writes them.
    public Dictionary<string, List<object>> GetIndexDocument(CatalogProduct product)
    {
        var outlines = GetOutlines(product);
        var visible = product.IsActive == true && IsActive(GetCategory(product.CategoryId));

        return new Dictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase)
        {
            ["__outline"] = outlines.SelectMany(ExpandOutline).Distinct().Cast<object>().ToList(),
            ["catalog"] = outlines.Select(x => x.Items.First().Id).Distinct().Cast<object>().ToList(),
            ["is"] = [string.IsNullOrEmpty(product.MainProductId) ? "product" : "variation", visible ? "visible" : "hidden", product.Code],
            ["status"] = [visible ? "visible" : "hidden"],
            ["startdate"] = [product.StartDate],
            ["enddate"] = [product.EndDate ?? DateTime.MaxValue],
            ["modifieddate"] = [IndexedModifiedDates.TryGetValue(product.Id, out var indexed) ? indexed : product.ModifiedDate ?? DateTime.MaxValue],
            ["__sort"] = [product.Name],
            ["__content"] = [product.Name, product.Code],
        };
    }

    public bool IsActive(Category category)
    {
        for (var current = category; current != null; current = current.ParentId == null ? null : GetCategory(current.ParentId))
        {
            if (current.IsActive != true)
            {
                return false;
            }
        }

        return true;
    }

    private List<OutlineItem> GetOwnItems(Category category)
    {
        var items = new List<OutlineItem>();

        for (var current = category; current != null; current = current.ParentId == null ? null : GetCategory(current.ParentId))
        {
            items.Insert(0, new OutlineItem { Id = current.Id, Name = current.Name });
        }

        items.Insert(0, CatalogItem(GetCatalog(category.CatalogId)));

        return items;
    }

    private static OutlineItem CatalogItem(Catalog catalog)
    {
        return new OutlineItem { Id = catalog.Id, Name = catalog.Name };
    }

    // "catalog/a/b", "catalog/a", "catalog": every parent of the item, in lower case, as the index keeps them.
    private static IEnumerable<string> ExpandOutline(Outline outline)
    {
        var ids = outline.Items.Take(outline.Items.Count - 1).Select(x => x.Id.ToLowerInvariant()).ToList();

        for (var i = ids.Count; i > 0; i--)
        {
            yield return string.Join("/", ids.Take(i));
        }
    }

    private static CatalogProduct CreateProduct(string id, string sku, string name, string catalogId, string categoryId, bool isActive = true)
    {
        return new CatalogProduct
        {
            Id = id,
            Code = sku,
            Name = name,
            CatalogId = catalogId,
            CategoryId = categoryId,
            IsActive = isActive,
            IsBuyable = true,
            TrackInventory = true,
            StartDate = Saved.AddDays(-30),
            ModifiedDate = Saved,
            Images = [new Image { Url = $"https://cdn.example.com/{id}.png" }],
            Reviews = [new EditorialReview { Content = $"About {name}.", LanguageCode = "en-US", ReviewType = "QuickReview" }],
            Properties = [],
        };
    }

    private static Store CreateStore(string id, string name, string catalogId)
    {
        return new Store
        {
            Id = id,
            Name = name,
            Catalog = catalogId,
            DefaultLanguage = "en-US",
            Languages = ["en-US"],
            DefaultCurrency = "USD",
            Currencies = ["USD"],
        };
    }
}
