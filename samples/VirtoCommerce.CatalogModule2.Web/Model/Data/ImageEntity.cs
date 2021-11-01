using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class ImageEntity2 : ImageEntity
    {
        public override void Patch(ImageEntity target)
        {
            base.Patch(target);
        }
        public override ImageEntity FromModel(Image image, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(image, pkMap);
        }
        public override Image ToModel(Image image)
        {
            return base.ToModel(image);
        }
    }
}
