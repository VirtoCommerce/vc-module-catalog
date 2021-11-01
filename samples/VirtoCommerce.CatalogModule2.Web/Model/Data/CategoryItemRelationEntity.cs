using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class CategoryItemRelationEntity2 : CategoryItemRelationEntity
    {
        public override CategoryLink ToModel(CategoryLink link)
        {
            return base.ToModel(link);
        }
        public override void Patch(CategoryItemRelationEntity target)
        {
            base.Patch(target);
        }
        public override CategoryItemRelationEntity FromModel(CategoryLink link)
        {
            return base.FromModel(link);
        }
    }
}
