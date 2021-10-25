using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model
{
    public class Variation2 : Variation
    {
        public override string SeoObjectType => base.SeoObjectType;
        public override void Move(string catalogId, string categoryId)
        {
            base.Move(catalogId, categoryId);
        }
        public override void TryInheritFrom(IEntity parent)
        {
            base.TryInheritFrom(parent);
        }
        public override void ReduceDetails(string responseGroup)
        {
            base.ReduceDetails(responseGroup);
        }
    }
}
