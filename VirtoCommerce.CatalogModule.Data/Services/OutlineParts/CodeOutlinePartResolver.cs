using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services.OutlineParts
{
    /// <summary>
    /// Uses codes for outline items.
    /// </summary>
    public class CodeOutlinePartResolver : IOutlinePartResolver
    {
        public string ResolveOutlinePart(Entity entity)
        {
            var category = entity as Category;
            if (category != null)
                return category.Code;

            var product = entity as CatalogProduct;
            if (product != null)
                return product.Code;

            // Fallback to id.
            return entity.Id;
        }
    }
}