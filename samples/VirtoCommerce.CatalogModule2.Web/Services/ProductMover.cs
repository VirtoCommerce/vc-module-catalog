using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class ProductMover2 : ProductMover
    {
        public ProductMover2(IItemService itemService) : base(itemService)
        {
        }
        public override Task ConfirmMoveAsync(IEnumerable<CatalogProduct> entities)
        {
            return base.ConfirmMoveAsync(entities);
        }
        public override Task<List<CatalogProduct>> PrepareMoveAsync(ListEntriesMoveRequest moveInfo)
        {
            return base.PrepareMoveAsync(moveInfo);
        }
    }
}
