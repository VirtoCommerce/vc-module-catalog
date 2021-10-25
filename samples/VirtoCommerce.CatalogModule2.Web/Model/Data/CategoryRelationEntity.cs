using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class CategoryRelationEntity2 : CategoryRelationEntity
    {
        public override CategoryRelationEntity FromModel(CategoryLink link)
        {
            return base.FromModel(link);
        }
        public override void Patch(CategoryRelationEntity target)
        {
            base.Patch(target);
        }
        public override CategoryLink ToModel(CategoryLink link)
        {
            return base.ToModel(link);
        }
    }
}
