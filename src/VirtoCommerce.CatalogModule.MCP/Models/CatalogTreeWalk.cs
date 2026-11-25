using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.MCP.Models;

// The categories a walk of a catalog's tree found (CatalogTree.WalkAsync), and what it did not show.
public class CatalogTreeWalk
{
    public IList<CatalogTreeNode> Nodes { get; set; } = [];

    // The deepest level that has categories shown, 1 for the catalog's root categories.
    public int DeepestLevel { get; set; }

    // Some categories are not shown: the walk reached its limit, or a category has more children than are read.
    public bool Cut { get; set; }

    // Some categories linked in belong to catalogs the user may not read.
    public bool LinksLeftOut { get; set; }
}
