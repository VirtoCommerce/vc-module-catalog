using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class ItemEntity2 : ItemEntity
    {
        public override CatalogProduct ToModel(CatalogProduct product, bool convertChildrens = true, bool convertAssociations = true)
        {
            return base.ToModel(product, convertChildrens, convertAssociations);
        }
        public override ItemEntity FromModel(CatalogProduct product, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(product, pkMap);
        }
        public override void Patch(ItemEntity target)
        {
            base.Patch(target);
        }
    }
}
