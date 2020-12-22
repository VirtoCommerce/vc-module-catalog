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

        public string ResolveOutlineName(IEntity entity)
        {
            var result = entity.Id;
            if (entity is Catalog catalog)
            {
                result = catalog.Name;
            }
            else if (entity is Category category)
            {
                result = category.Name;
            }

            return result;
        }
    }
}
