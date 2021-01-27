using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.OutlinePart
{
    /// <summary>
    /// Uses ids for oultines.
    /// </summary>
    public class IdOutlinePartResolver : IOutlinePartResolver
    {
        public string ResolveOutlinePart(IEntity entity)
        {
            return entity.Id;
        }
    }
}
