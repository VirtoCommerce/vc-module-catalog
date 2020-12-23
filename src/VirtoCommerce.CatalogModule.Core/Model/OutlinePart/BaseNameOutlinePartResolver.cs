using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.OutlinePart
{
    public abstract class BaseNameOutlinePartResolver : IOutlinePartResolver
    {
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

        public abstract string ResolveOutlinePart(IEntity entity);
    }
}
