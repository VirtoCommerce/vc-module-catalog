using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class CategoryMover2 : CategoryMover
    {
        public CategoryMover2(ICategoryService categoryService) : base(categoryService)
        {
        }
        public override Task ConfirmMoveAsync(IEnumerable<Category> entities)
        {
            return base.ConfirmMoveAsync(entities);
        }
        public override Task<List<Category>> PrepareMoveAsync(ListEntriesMoveRequest moveInfo)
        {
            return base.PrepareMoveAsync(moveInfo);
        }
        protected override Task ValidateOperationArguments(ListEntriesMoveRequest moveInfo)
        {
            return base.ValidateOperationArguments(moveInfo);
        }
    }
}
