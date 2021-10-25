using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class CategoryEntity2 : CategoryEntity
    {
        public override CategoryEntity FromModel(Category category, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(category, pkMap);
        }
        public override void Patch(CategoryEntity target)
        {
            base.Patch(target);
        }
        public override Category ToModel(Category category)
        {
            return base.ToModel(category);
        }
    }
}
