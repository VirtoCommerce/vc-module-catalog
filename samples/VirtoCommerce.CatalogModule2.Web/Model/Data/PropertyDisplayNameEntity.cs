using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class PropertyDisplayNameEntity2 : PropertyDisplayNameEntity
    {
        public override PropertyDisplayName ToModel(PropertyDisplayName displayName)
        {
            return base.ToModel(displayName);
        }
        public override PropertyDisplayNameEntity FromModel(PropertyDisplayName displayName)
        {
            return base.FromModel(displayName);
        }
        public override void Patch(PropertyDisplayNameEntity target)
        {
            base.Patch(target);
        }
    }
}
