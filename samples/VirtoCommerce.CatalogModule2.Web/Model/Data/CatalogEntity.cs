using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class CatalogEntity2 : CatalogEntity
    {
        public override Catalog ToModel(Catalog catalog)
        {
            return base.ToModel(catalog);
        }
        public override CatalogEntity FromModel(Catalog catalog, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(catalog, pkMap);
        }
        public override void Patch(CatalogEntity target)
        {
            base.Patch(target);
        }
    }
}
