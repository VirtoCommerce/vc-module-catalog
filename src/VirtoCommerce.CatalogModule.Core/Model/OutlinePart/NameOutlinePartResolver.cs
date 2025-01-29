using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.OutlinePart
{
    public class NameOutlinePartResolver : IOutlinePartNameResolver
    {
        public LocalizedString ResolveLocalizedOutlineName(IEntity entity)
        {
            if (entity is Category category)
            {
                return category.LocalizedName ?? new LocalizedString();
            }
            else if (entity is CatalogProduct product)
            {
                return product.LocalizedName ?? new LocalizedString();
            }

            return new LocalizedString();
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
            else if (entity is CatalogProduct product)
            {
                result = product.Name;
            }

            return result;
        }
    }
}
