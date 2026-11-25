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

// A catalog's categories as customers browse it: under each category, its subcategories and the categories linked into it
// from other catalogs, which the user may read. Each step reads the categories of one level, so the cost grows with the
// categories shown, never with the catalog. A walk keeps the categories it has seen, as links saved through the API may
// close a loop.
public class CatalogTree(
    ICategorySearchService categorySearchService,
    ICategoryService categoryService,
    ILinkSearchService linkSearchService,
    CatalogAccess access)
{
    // The most categories read under one category; a category with more is cut, and the answer says so.
    public const int MaxChildrenPerCategory = 500;

    public const string PathSeparator = " > ";

    // The categories directly under a node, or under the catalog's root without one, ordered by name. linksLeftOut: some
    // categories linked in belong to catalogs the user may not read.
    public virtual async Task<(IList<CatalogTreeNode> Children, bool LinksLeftOut, bool Cut)> GetChildrenAsync(ClaimsPrincipal user, Catalog catalog,
        CatalogTreeNode parent, string responseGroup)
    {
        var criteria = AbstractTypeFactory<CategorySearchCriteria>.TryCreateInstance();
        criteria.CatalogId = parent == null ? catalog.Id : parent.Category.CatalogId;
        criteria.CategoryId = parent?.Category.Id;
        criteria.SearchOnlyInRoot = parent == null;
        criteria.ResponseGroup = responseGroup;
        criteria.Take = MaxChildrenPerCategory + 1;
        // A store's tree may be a catalog the user may not read, whose own categories stay hidden too.
        var own = await access.FilterAllowedAsync(user, (await categorySearchService.SearchAsync(criteria)).Results, CatalogPermissions.CategoriesRead);

        var linkCriteria = AbstractTypeFactory<LinkSearchCriteria>.TryCreateInstance();
        linkCriteria.ObjectType = nameof(Category);
        linkCriteria.Take = MaxChildrenPerCategory + 1;

        if (parent == null)
        {
            // Links into the catalog's root.
            linkCriteria.CatalogIds = [catalog.Id];
        }
        else
        {
            linkCriteria.CategoryIds = [parent.Category.Id];
        }

        var linkedIds = (await linkSearchService.SearchAsync(linkCriteria)).Results.Select(x => x.ListEntryId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var linked = linkedIds.Count == 0 ? [] : await categoryService.GetAsync(linkedIds, responseGroup);
        var readableLinked = await access.FilterAllowedAsync(user, linked, CatalogPermissions.CategoriesRead);

        var children = own.Select(x => CreateNode(catalog, parent, x, isLinked: false))
            .Concat(readableLinked.Select(x => CreateNode(catalog, parent, x, isLinked: true)))
            .OrderBy(x => x.Category.Name, StringComparer.OrdinalIgnoreCase)
            .Take(MaxChildrenPerCategory)
            .ToList();
        var cut = own.Count + readableLinked.Count > MaxChildrenPerCategory;

        return (children, readableLinked.Count < linked.Count, cut);
    }

    // The categories under a node, or under the catalog's root, level by level, at most maxCount of them and maxDepth levels
    // down; each node after its parent.
    public virtual async Task<CatalogTreeWalk> WalkAsync(ClaimsPrincipal user, Catalog catalog, CatalogTreeNode start, int maxDepth, int maxCount, string responseGroup)
    {
        var walk = new CatalogTreeWalk();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var level = new List<CatalogTreeNode> { start };

        if (start != null)
        {
            seen.Add(start.Category.Id);
        }

        for (var depth = 1; depth <= maxDepth && level.Count > 0; depth++)
        {
            var next = new List<CatalogTreeNode>();

            foreach (var parent in level)
            {
                var (children, linksLeftOut, cut) = await GetChildrenAsync(user, catalog, parent, responseGroup);
                walk.LinksLeftOut |= linksLeftOut;
                walk.Cut |= cut;

                foreach (var child in children.Where(x => seen.Add(x.Category.Id)))
                {
                    if (walk.Nodes.Count >= maxCount)
                    {
                        walk.Cut = true;
                        return walk;
                    }

                    walk.Nodes.Add(child);
                    next.Add(child);
                }
            }

            walk.DeepestLevel = next.Count > 0 ? depth : walk.DeepestLevel;
            level = next;
        }

        return walk;
    }

    // A category of the catalog's tree by its path ("Printers > Laser", names ignoring case) or by its id. Refused when the
    // user may not read it or it is not in the catalog's tree, with the same words, so an answer tells nothing about
    // categories the user may not read.
    public virtual async Task<CatalogTreeNode> FindAsync(ClaimsPrincipal user, Catalog catalog, string pathOrId, string responseGroup)
    {
        if (string.IsNullOrWhiteSpace(pathOrId))
        {
            return null;
        }

        var value = pathOrId.Trim();

        if (!value.Contains('>'))
        {
            var byId = await FindByIdAsync(user, catalog, value, responseGroup);

            if (byId != null)
            {
                return byId;
            }
        }

        return await FindByPathAsync(user, catalog, value, responseGroup)
            ?? throw new McpException($"Category '{pathOrId}' was not found. Give its path as customers browse the catalog, such as " +
                "'Printers > Laser', or its id.");
    }

    protected virtual async Task<CatalogTreeNode> FindByIdAsync(ClaimsPrincipal user, Catalog catalog, string categoryId, string responseGroup)
    {
        var category = (await categoryService.GetAsync([categoryId], $"{responseGroup},{CategoryResponseGroup.WithOutlines}")).FirstOrDefault();

        if (category == null || !await access.IsAllowedAsync(user, category, CatalogPermissions.CategoriesRead))
        {
            return null;
        }

        var outline = category.Outlines?.FirstOrDefault(x => x.Items?.FirstOrDefault()?.Id.EqualsIgnoreCase(catalog.Id) == true);

        if (outline == null)
        {
            return null;
        }

        var items = outline.Items.ToList();
        var node = AbstractTypeFactory<CatalogTreeNode>.TryCreateInstance();
        node.Category = category;
        node.OutlineIds = items.Select(x => x.Id).ToList();
        node.Path = string.Join(PathSeparator, items.Skip(1).Select(x => x.Name));
        node.Depth = items.Count - 1;
        node.IsLinked = !category.CatalogId.EqualsIgnoreCase(catalog.Id);

        return node;
    }

    // Walks the path from the catalog's root, one level at a time. A name that two categories of a level share is refused
    // with their ids.
    protected virtual async Task<CatalogTreeNode> FindByPathAsync(ClaimsPrincipal user, Catalog catalog, string path, string responseGroup)
    {
        var names = path.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        CatalogTreeNode node = null;

        foreach (var name in names)
        {
            var (children, _, _) = await GetChildrenAsync(user, catalog, node, responseGroup);
            var matches = children.Where(x => x.Category.Name.EqualsIgnoreCase(name)).ToList();

            if (matches.Count > 1)
            {
                throw new McpException($"'{string.Join(PathSeparator, names)}' names {matches.Count} categories: give the id of one of " +
                    $"{string.Join(", ", matches.Select(x => x.Category.Id))}.");
            }

            node = matches.FirstOrDefault();

            if (node == null)
            {
                return null;
            }
        }

        return node;
    }

    protected virtual CatalogTreeNode CreateNode(Catalog catalog, CatalogTreeNode parent, Category category, bool isLinked)
    {
        var node = AbstractTypeFactory<CatalogTreeNode>.TryCreateInstance();
        node.Category = category;
        node.OutlineIds = (parent?.OutlineIds ?? [catalog.Id]).Append(category.Id).ToList();
        node.Path = parent == null ? category.Name : $"{parent.Path}{PathSeparator}{category.Name}";
        node.Depth = (parent?.Depth ?? 0) + 1;
        node.IsLinked = isLinked;

        return node;
    }
}
