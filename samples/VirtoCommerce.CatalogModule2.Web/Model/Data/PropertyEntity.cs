using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyEntity2 : PropertyEntity
    {
        public override void Patch(PropertyEntity target)
        {
            base.Patch(target);
        }
        public override PropertyEntity FromModel(Property property, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(property, pkMap);
        }
        public override Property ToModel(Property property)
        {
            return base.ToModel(property);
        }
    }
}
