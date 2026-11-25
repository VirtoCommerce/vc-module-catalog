using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.MCP.Models;

// A category as customers browse a catalog: by its subcategories and the categories linked into it (CatalogTree).
public class CatalogTreeNode
{
    public Category Category { get; set; }

    // The names from the browsed catalog down to the category: "Printers > Inkjet Printers".
    public string Path { get; set; }

    // The ids from the browsed catalog down to the category, the catalog first: the category's outline there.
    public IList<string> OutlineIds { get; set; } = [];

    // 1 for a category at the catalog's root.
    public int Depth { get; set; }

    // Linked into its parent, or into the catalog's root, rather than a subcategory of it.
    public bool IsLinked { get; set; }

    // The outline as the search index keeps it, in lower case: "catalog/category/category".
    public string Outline => string.Join("/", OutlineIds).ToLowerInvariant();
}
