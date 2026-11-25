using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.CatalogModule.MCP.Services;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.Platform.Core.Common;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.MCP.Tools;

// A catalog's categories and properties, and what a virtual catalog links. A category tree and its properties belong to the
// catalog, whichever stores sell from it, so these tools take the catalog by its id.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(CatalogPermissions.Read)]
public class CatalogStructureTools(
    CatalogAccess access,
    CatalogTree tree,
    CatalogReach reach,
    CatalogBackOfficeLinks links,
    ICatalogService catalogService,
    IItemService itemService,
    IProductSearchService productSearchService,
    ILinkSearchService linkSearchService,
    IPropertyDictionaryItemSearchService dictionaryItemSearchService)
{
    // The most categories in one answer: category opens a branch, depth limits the levels.
    public const int MaxCategories = 200;
    public const int DefaultDepth = 2;
    public const int MaxDepth = 10;

    // The most values listed per dictionary property.
    public const int MaxDictionaryValues = 20;

    // The most products linked into a virtual catalog listed in one answer.
    public const int MaxProductLinks = 200;

    public const string LinksLeftOutNote = "Some linked categories or products belong to catalogs you may not read: they are not shown.";

    public const string InheritanceNote = "A product has the properties of its catalog and of every category above it; a category's " +
        "row lists only the properties it adds.";

    [McpServerTool(Name = "get_catalog_structure", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("A catalog's category tree, level by level: each category's name, path, id and whether it is active, the " +
        "categories linked into it from other catalogs, and the properties each one adds, with their type, whether they are " +
        "required and a dictionary's values; the catalog's own properties; and the stores that sell from the catalog. At most " +
        "200 categories per answer: category opens a branch, depth limits the levels. get_catalog_overview names the catalogs.")]
    public async Task<object> GetCatalogStructure(
        RequestContext<CallToolRequestParams> context,
        [Description("Catalog id, as get_catalog_overview names it.")] string catalog_id,
        [Description("A category to open, by its path as this tool shows it, such as 'Printers > Laser', or by its id; left out, " +
            "the catalog's root.")] string category = null,
        [Description("Levels of categories to show below the root or the category, from 1 to 10; 2 when left out.")] int? depth = null)
    {
        var user = context.User;
        var catalog = await access.GetReadableCatalogAsync(user, catalog_id);
        await access.EnsureAllowedAsync(user, catalog, CatalogPermissions.CategoriesRead,
            $"Your {CatalogPermissions.CategoriesRead} permission does not cover catalog '{catalog_id}'.");
        // The catalog's own properties come with it.
        catalog = await catalogService.GetByIdAsync(catalog.Id, CatalogResponseGroup.Full.ToString());

        var responseGroup = $"{CategoryResponseGroup.Info},{CategoryResponseGroup.WithProperties}";
        var start = await tree.FindAsync(user, catalog, category, responseGroup);
        var levels = Math.Clamp(depth ?? DefaultDepth, 1, MaxDepth);
        var walk = await tree.WalkAsync(user, catalog, start, levels, MaxCategories, responseGroup);
        var catalogNames = await GetCatalogNamesAsync(walk.Nodes.Where(x => x.IsLinked).Select(x => x.Category.CatalogId));
        var catalogProperties = (catalog.Properties ?? []).Where(x => string.IsNullOrEmpty(x.CategoryId)).ToList();
        var categoryProperties = walk.Nodes.ToDictionary(x => x.Category.Id, x => GetOwnProperties(x.Category), StringComparer.OrdinalIgnoreCase);
        var dictionaryValues = await GetDictionaryValuesAsync(catalogProperties.Concat(categoryProperties.Values.SelectMany(x => x)));
        var storesByCatalog = await reach.GetStoresByCatalogAsync();
        var defaultLanguage = catalog.DefaultLanguage?.LanguageCode;

        return new
        {
            catalog = new
            {
                id = catalog.Id,
                name = catalog.Name,
                @virtual = catalog.IsVirtual,
                default_language = defaultLanguage,
                languages = (catalog.Languages ?? []).Select(x => x.LanguageCode).ToList(),
            },
            sold_by = reach.Describe(user, storesByCatalog, catalog.Id),
            properties = catalogProperties.Select(x => GetPropertyRow(x, defaultLanguage, dictionaryValues)).ToList(),
            opened = start?.Path,
            categories = walk.Nodes.Select(x => new
            {
                id = x.Category.Id,
                name = x.Category.Name,
                path = x.Path,
                active = x.Category.IsActive != false,
                linked_from = x.IsLinked ? catalogNames.GetValueOrDefault(x.Category.CatalogId, x.Category.CatalogId) : null,
                properties = categoryProperties[x.Category.Id].Select(p => GetPropertyRow(p, defaultLanguage, dictionaryValues)).ToList() is { Count: > 0 } rows ? rows : null,
            }).ToList(),
            shown = GetShownText(walk, levels),
            links_left_out = walk.LinksLeftOut ? LinksLeftOutNote : null,
            inheritance = InheritanceNote,
            back_office = await links.GetCatalogLinkAsync(),
            next_step = catalog.IsVirtual
                ? "get_virtual_catalog shows what this virtual catalog links, with product counts."
                : "search_products and get_product show the products of a store that sells from this catalog.",
        };
    }

    [McpServerTool(Name = "get_virtual_catalog", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("A virtual catalog's own categories, each with what is linked into it: categories of physical catalogs (their " +
        "path and catalog, and how many products they bring in) and single products (SKU and name); and the stores that sell " +
        "from it. At most 200 categories per answer: category opens a branch, depth limits the levels.")]
    public async Task<object> GetVirtualCatalog(
        RequestContext<CallToolRequestParams> context,
        [Description("Id of a virtual catalog, as get_catalog_overview names it.")] string catalog_id,
        [Description("An own category of the virtual catalog to open, by its path, such as 'Printers', or by its id; left out, the " +
            "catalog's root.")] string category = null,
        [Description("Levels of own categories to show below the root or the category, from 1 to 10; 2 when left out.")] int? depth = null)
    {
        var user = context.User;
        var catalog = await access.GetReadableCatalogAsync(user, catalog_id);

        if (!catalog.IsVirtual)
        {
            throw new McpException($"Catalog '{catalog.Name}' is a physical catalog: get_catalog_structure shows its categories.");
        }

        await access.EnsureAllowedAsync(user, catalog, CatalogPermissions.CategoriesRead,
            $"Your {CatalogPermissions.CategoriesRead} permission does not cover catalog '{catalog_id}'.");

        var responseGroup = $"{CategoryResponseGroup.Info},{CategoryResponseGroup.WithOutlines}";
        var start = await tree.FindAsync(user, catalog, category, responseGroup);

        if (start?.IsLinked == true)
        {
            throw new McpException($"'{category}' is linked into the virtual catalog from another catalog: get_catalog_structure with that " +
                "catalog's id shows its subtree.");
        }

        var levels = Math.Clamp(depth ?? DefaultDepth, 1, MaxDepth);
        var walk = await WalkOwnCategoriesAsync(user, catalog, start, levels, responseGroup);
        var productLinks = await GetProductLinksAsync(user, catalog, walk.Walk.Nodes.Select(x => x.Category.Id).ToList(), includeRoot: start == null);
        var linkedCategories = walk.Links.Values.SelectMany(x => x).ToList();
        var catalogNames = await GetCatalogNamesAsync(linkedCategories.Select(x => x.CatalogId).Concat(productLinks.Products.Select(x => x.CatalogId)));
        var storesByCatalog = await reach.GetStoresByCatalogAsync();

        return new
        {
            catalog = new { id = catalog.Id, name = catalog.Name, languages = (catalog.Languages ?? []).Select(x => x.LanguageCode).ToList() },
            sold_by = reach.Describe(user, storesByCatalog, catalog.Id),
            opened = start?.Path,
            root_links = start == null
                ? await GetLinkRowsAsync(walk.Links.GetValueOrDefault(string.Empty) ?? [], productLinks.ByCategory.GetValueOrDefault(string.Empty) ?? [], catalogNames)
                : null,
            categories = await GetCategoryRowsAsync(walk, productLinks, catalogNames),
            shown = GetShownText(walk.Walk, levels),
            product_links_cut = productLinks.Cut ? $"Only the first {MaxProductLinks} linked products are listed." : null,
            links_left_out = walk.Walk.LinksLeftOut || productLinks.LeftOut ? LinksLeftOutNote : null,
            back_office = await links.GetCatalogLinkAsync(),
            next_step = "get_catalog_structure with a linked category's catalog id shows that catalog's categories and properties.",
        };
    }

    // The virtual catalog's own categories, level by level, and the categories linked into each one (or into the root,
    // under the empty key), which are listed as links and not opened.
    protected virtual async Task<(CatalogTreeWalk Walk, Dictionary<string, IList<Category>> Links)> WalkOwnCategoriesAsync(
        ClaimsPrincipal user, Catalog catalog, CatalogTreeNode start, int levels, string responseGroup)
    {
        var walk = new CatalogTreeWalk();
        var linksByParent = new Dictionary<string, IList<Category>>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var level = new List<CatalogTreeNode> { start };

        for (var depth = 1; depth <= levels && level.Count > 0; depth++)
        {
            var next = new List<CatalogTreeNode>();

            foreach (var parent in level)
            {
                var (children, linksLeftOut, cut) = await tree.GetChildrenAsync(user, catalog, parent, responseGroup);
                walk.LinksLeftOut |= linksLeftOut;
                walk.Cut |= cut;
                linksByParent[parent?.Category.Id ?? string.Empty] = children.Where(x => x.IsLinked).Select(x => x.Category).ToList();

                foreach (var child in children.Where(x => !x.IsLinked && seen.Add(x.Category.Id)))
                {
                    if (walk.Nodes.Count >= MaxCategories)
                    {
                        walk.Cut = true;
                        return (walk, linksByParent);
                    }

                    walk.Nodes.Add(child);
                    next.Add(child);
                }
            }

            walk.DeepestLevel = next.Count > 0 ? depth : walk.DeepestLevel;
            level = next;
        }

        return (walk, linksByParent);
    }

    // The products linked into the categories, and into the catalog's root when asked, that the user may read: by the id of
    // the category they are linked into, the empty key for the root.
    protected virtual async Task<(IList<CatalogProduct> Products, Dictionary<string, IList<CatalogProduct>> ByCategory, bool Cut, bool LeftOut)> GetProductLinksAsync(
        ClaimsPrincipal user, Catalog catalog, IList<string> categoryIds, bool includeRoot)
    {
        var found = new List<CategoryLink>();
        var cut = false;

        foreach (var criteria in GetProductLinkCriteria(catalog, categoryIds, includeRoot))
        {
            var result = await linkSearchService.SearchAsync(criteria);
            found.AddRange(result.Results);
            cut |= result.TotalCount > result.Results.Count;
        }

        found = found.Take(MaxProductLinks).ToList();
        cut |= found.Count >= MaxProductLinks;

        var productIds = found.Select(x => x.ListEntryId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var products = productIds.Count == 0 ? [] : await itemService.GetAsync(productIds, ItemResponseGroup.ItemInfo.ToString());
        var readable = await access.FilterAllowedAsync(user, products, CatalogPermissions.ProductsRead);
        var readableById = readable.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var byCategory = found
            .Where(x => readableById.ContainsKey(x.ListEntryId))
            .GroupBy(x => x.CategoryId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => (IList<CatalogProduct>)x.Select(link => readableById[link.ListEntryId]).ToList(), StringComparer.OrdinalIgnoreCase);

        return (readable, byCategory, cut, readable.Count < products.Count);
    }

    protected virtual IEnumerable<LinkSearchCriteria> GetProductLinkCriteria(Catalog catalog, IList<string> categoryIds, bool includeRoot)
    {
        if (categoryIds.Count > 0)
        {
            var criteria = AbstractTypeFactory<LinkSearchCriteria>.TryCreateInstance();
            criteria.ObjectType = nameof(CatalogProduct);
            criteria.CategoryIds = categoryIds;
            criteria.Take = MaxProductLinks;
            yield return criteria;
        }

        if (includeRoot)
        {
            var criteria = AbstractTypeFactory<LinkSearchCriteria>.TryCreateInstance();
            criteria.ObjectType = nameof(CatalogProduct);
            criteria.CatalogIds = [catalog.Id];
            criteria.Take = MaxProductLinks;
            yield return criteria;
        }
    }

    protected virtual async Task<IList<object>> GetCategoryRowsAsync(
        (CatalogTreeWalk Walk, Dictionary<string, IList<Category>> Links) walk,
        (IList<CatalogProduct> Products, Dictionary<string, IList<CatalogProduct>> ByCategory, bool Cut, bool LeftOut) productLinks,
        Dictionary<string, string> catalogNames)
    {
        var rows = new List<object>();

        foreach (var node in walk.Walk.Nodes)
        {
            rows.Add(new
            {
                id = node.Category.Id,
                name = node.Category.Name,
                path = node.Path,
                active = node.Category.IsActive != false,
                links = await GetLinkRowsAsync(walk.Links.GetValueOrDefault(node.Category.Id) ?? [],
                    productLinks.ByCategory.GetValueOrDefault(node.Category.Id) ?? [], catalogNames),
            });
        }

        return rows;
    }

    // A linked category with its path in its own catalog and the products it brings in; a linked product with its SKU.
    protected virtual async Task<IList<object>> GetLinkRowsAsync(IList<Category> categories, IList<CatalogProduct> products, Dictionary<string, string> catalogNames)
    {
        var rows = new List<object>();

        foreach (var category in categories)
        {
            rows.Add(new
            {
                category_id = category.Id,
                catalog = catalogNames.GetValueOrDefault(category.CatalogId, category.CatalogId),
                catalog_id = category.CatalogId,
                path = GetOwnPath(category),
                products = await CountProductsAsync(category),
            });
        }

        rows.AddRange(products.Select(x => new
        {
            sku = x.Code,
            name = x.Name,
            catalog = catalogNames.GetValueOrDefault(x.CatalogId, x.CatalogId),
            catalog_id = x.CatalogId,
        }));

        return rows;
    }

    // The products of the category's own catalog in its subtree, in the database: the cost grows with the subtree, not the
    // catalog. Products of other catalogs linked into the subtree are not counted.
    protected virtual async Task<int> CountProductsAsync(Category category)
    {
        var criteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();
        criteria.CatalogIds = [category.CatalogId];
        criteria.CategoryIds = [category.Id];
        criteria.SearchInChildren = true;
        criteria.Take = 0;

        return (await productSearchService.SearchNoCloneAsync(criteria)).TotalCount;
    }

    // The category's path in its own catalog: "Printers > Inkjet Printers".
    protected virtual string GetOwnPath(Category category)
    {
        var outline = category.Outlines?.FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(category.CatalogId) == true);

        return outline == null ? category.Name : string.Join(CatalogTree.PathSeparator, outline.Items.Skip(1).Select(x => x.Name));
    }

    // The properties a category adds: its own, not the ones it inherits.
    protected virtual IList<Property> GetOwnProperties(Category category)
    {
        return (category.Properties ?? []).Where(x => !x.IsInherited && x.CategoryId.EqualsIgnoreCase(category.Id)).ToList();
    }

    protected virtual object GetPropertyRow(Property property, string language, Dictionary<string, (IList<string> Values, int Total)> dictionaryValues)
    {
        var values = property.Dictionary && property.Id != null ? dictionaryValues.GetValueOrDefault(property.Id) : default;

        return new
        {
            name = property.Name,
            display_name = property.DisplayNames?.FirstOrDefault(x => x.LanguageCode.EqualsIgnoreCase(language))?.Name
                ?? property.DisplayNames?.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name))?.Name,
            type = JsonNamingPolicy.SnakeCaseLower.ConvertName(property.ValueType.ToString()),
            applies_to = JsonNamingPolicy.SnakeCaseLower.ConvertName(property.Type.ToString()),
            required = property.Required,
            multivalue = property.Multivalue,
            multilanguage = property.Multilanguage,
            values = values.Values,
            values_cut = values.Total > MaxDictionaryValues ? $"{MaxDictionaryValues} of {values.Total} values listed." : null,
        };
    }

    // The first values of each dictionary property, by alias, and how many it has.
    protected virtual async Task<Dictionary<string, (IList<string> Values, int Total)>> GetDictionaryValuesAsync(IEnumerable<Property> properties)
    {
        var result = new Dictionary<string, (IList<string> Values, int Total)>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in properties.Where(x => x.Dictionary && x.Id != null).DistinctBy(x => x.Id))
        {
            var criteria = AbstractTypeFactory<PropertyDictionaryItemSearchCriteria>.TryCreateInstance();
            criteria.PropertyIds = [property.Id];
            criteria.Take = MaxDictionaryValues;
            var items = await dictionaryItemSearchService.SearchNoCloneAsync(criteria);
            result[property.Id] = (items.Results.Select(x => x.Alias).ToList(), items.TotalCount);
        }

        return result;
    }

    protected virtual async Task<Dictionary<string, string>> GetCatalogNamesAsync(IEnumerable<string> catalogIds)
    {
        var ids = catalogIds.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var catalogs = ids.Count == 0 ? [] : await catalogService.GetNoCloneAsync(ids, CatalogResponseGroup.Info.ToString());

        return catalogs.ToDictionary(x => x.Id, x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    // "120 categories in 2 levels", and what was cut.
    protected virtual string GetShownText(CatalogTreeWalk walk, int levels)
    {
        var count = walk.Nodes.Count == 1 ? "1 category" : $"{walk.Nodes.Count} categories";
        var text = $"{count} in {walk.DeepestLevel} level{(walk.DeepestLevel == 1 ? string.Empty : "s")}.";

        if (walk.Cut)
        {
            text += $" Not every category is shown: an answer holds at most {MaxCategories}; open a branch with category.";
        }
        else if (walk.DeepestLevel >= levels)
        {
            text += $" Categories below level {levels} are not shown: open a branch with category, or raise depth.";
        }

        return text;
    }
}
