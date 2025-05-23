using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class SeoInfoEntity2 : SeoInfoEntity
    {
        public override SeoInfo ToModel(SeoInfo seoInfo)
        {
            return base.ToModel(seoInfo);
        }
        public override SeoInfoEntity FromModel(SeoInfo seoInfo, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(seoInfo, pkMap);
        }
        public override void Patch(SeoInfoEntity target)
        {
            base.Patch(target);
        }
    }
}
