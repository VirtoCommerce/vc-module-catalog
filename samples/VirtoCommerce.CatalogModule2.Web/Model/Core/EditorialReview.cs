using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model
{
    public class EditorialReview2 : EditorialReview
    {
        public override bool ShouldSerializeAuditableProperties => base.ShouldSerializeAuditableProperties;
        public override void TryInheritFrom(IEntity parent)
        {
            base.TryInheritFrom(parent);
        }
    }
}
