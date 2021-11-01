using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyDictionaryItemEntity2 : PropertyDictionaryItemEntity
    {
        public override PropertyDictionaryItem ToModel(PropertyDictionaryItem propDictItem)
        {
            return base.ToModel(propDictItem);
        }
        public override PropertyDictionaryItemEntity FromModel(PropertyDictionaryItem propDictItem, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(propDictItem, pkMap);
        }
        public override void Patch(PropertyDictionaryItemEntity target)
        {
            base.Patch(target);
        }
    }
}
