using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.OutlinePart
{
    public class NameOutlinePartResolver : IOutlinePartNameResolver
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
            else if (entity is CatalogProduct product)
            {
                result = product.Name;
            }

            return result;
        }
    }
}
