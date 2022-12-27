using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class CatalogLanguageEntity2 : CatalogLanguageEntity
    {
        public override CatalogLanguageEntity FromModel(CatalogLanguage lang, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(lang, pkMap);
        }
        public override void Patch(CatalogLanguageEntity target)
        {
            base.Patch(target);
        }
        public override CatalogLanguage ToModel(CatalogLanguage lang)
        {
            return base.ToModel(lang);
        }
    }
}
