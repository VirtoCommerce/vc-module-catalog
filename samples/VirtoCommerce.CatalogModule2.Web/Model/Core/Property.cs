using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model
{
    public class Property2 : Property
    {
        public override bool IsInherited { get => base.IsInherited; set => base.IsInherited = value; }
        public override bool IsSame(Property other, params PropertyType[] additionalTypes)
        {
            return base.IsSame(other, additionalTypes);
        }
        public override bool IsSuitableForValue(PropertyValue propValue)
        {
            return base.IsSuitableForValue(propValue);
        }

        public override void TryInheritFrom(IEntity parent)
        {
            base.TryInheritFrom(parent);
        }
    }
}
