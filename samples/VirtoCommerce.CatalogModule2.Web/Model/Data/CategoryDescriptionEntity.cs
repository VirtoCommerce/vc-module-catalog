using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class CategoryDescriptionEntity2 : CategoryDescriptionEntity
    {
        public override CategoryDescription ToModel(CategoryDescription description)
        {
            return base.ToModel(description);
        }
        public override void Patch(CategoryDescriptionEntity target)
        {
            base.Patch(target);
        }
        public override CategoryDescriptionEntity FromModel(CategoryDescription description, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(description, pkMap);
        }
    }
}

