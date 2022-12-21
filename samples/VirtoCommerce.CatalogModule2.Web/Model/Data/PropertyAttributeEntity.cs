using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyAttributeEntity2 : PropertyAttributeEntity
    {
        public override void Patch(PropertyAttributeEntity target)
        {
            base.Patch(target);
        }
        public override PropertyAttributeEntity FromModel(PropertyAttribute attribute, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(attribute, pkMap);
        }
        public override PropertyAttribute ToModel(PropertyAttribute attribute)
        {
            return base.ToModel(attribute);
        }
    }
}
