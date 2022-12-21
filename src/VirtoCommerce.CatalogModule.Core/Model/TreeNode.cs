using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model;

public class TreeNode : Entity
{
    public IList<string> ChildIds { get; init; }
}
