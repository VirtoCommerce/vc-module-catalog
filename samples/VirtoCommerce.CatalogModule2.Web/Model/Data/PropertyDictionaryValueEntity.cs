using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyDictionaryValueEntity2 : PropertyDictionaryValueEntity
    {
        public override void Patch(PropertyDictionaryValueEntity target)
        {
            base.Patch(target);
        }
        public override PropertyDictionaryValueEntity FromModel(PropertyDictionaryItemLocalizedValue localizedValue, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(localizedValue, pkMap);
        }
        public override PropertyDictionaryItemLocalizedValue ToModel(PropertyDictionaryItemLocalizedValue localizedValue)
        {
            return base.ToModel(localizedValue);
        }
    }
}
