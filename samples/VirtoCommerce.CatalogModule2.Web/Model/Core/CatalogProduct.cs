using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Core.Model
{
    public class CatalogProduct2 : CatalogProduct
    {
        public override void ReduceDetails(string responseGroup)
        {
            base.ReduceDetails(responseGroup);
        }

        public override void Move(string catalogId, string categoryId)
        {
            base.Move(catalogId, categoryId);
        }

        public override void TryInheritFrom(IEntity parent)
        {
            base.TryInheritFrom(parent);
        }
    }
}
