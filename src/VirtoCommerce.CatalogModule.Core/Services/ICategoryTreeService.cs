using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Core.Services;

public interface ICategoryTreeService
{
    Task<IList<TreeNode>> GetNodesWithChildren(string catalogId, IList<string> ids, bool onlyActive);
}
