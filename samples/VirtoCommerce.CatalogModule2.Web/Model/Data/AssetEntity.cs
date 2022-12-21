using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class AssetEntity2 : AssetEntity
    {
        public override ItemEntity CatalogItem { get => base.CatalogItem; set => base.CatalogItem = value; }

        public override AssetEntity FromModel(Asset asset, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(asset, pkMap);
        }

        public override Asset ToModel(Asset asset)
        {
            return base.ToModel(asset);
        }
        public override void Patch(AssetEntity target)
        {
            base.Patch(target);
        }

    }
}
