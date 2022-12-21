using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model
{
    public class PropertyValue2 : PropertyValue
    {
        public override void TryInheritFrom(IEntity parent)
        {
            base.TryInheritFrom(parent);
        }
        public override bool IsEmpty => base.IsEmpty;
    }
}
