using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.OutlinePart
{
    public interface IOutlinePartNameResolver
    {
        string ResolveOutlineName(IEntity entity);

        IDictionary<string, string> ResolveLocalizedOutlineName(IEntity entity);
    }
}
