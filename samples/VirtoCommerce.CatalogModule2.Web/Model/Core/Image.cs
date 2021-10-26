using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model
{
    public class Image2 : Image
    {
        public override void TryInheritFrom(IEntity parent)
        {
            base.TryInheritFrom(parent);
        }

        public override bool ShouldSerializeAuditableProperties => base.ShouldSerializeAuditableProperties;
    }
}
