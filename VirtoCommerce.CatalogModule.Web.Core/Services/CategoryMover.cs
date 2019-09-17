using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Services
{
    public class CategoryMover : IListEntryMover<domain.Category>
    {
        private readonly ICategoryService _categoryService;

        public CategoryMover(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public void ConfirmMove(IEnumerable<domain.Category> entities)
        {
            if (entities.Any())
            {
                _categoryService.Update(entities.ToArray());
            }
        }

        public List<domain.Category> PrepareMove(MoveInfo moveInfo)
        {
            var result = new List<domain.Category>();

            foreach (var listEntryCategory in moveInfo.ListEntries.Where(x => x.Type.EqualsInvariant(ListEntryCategory.TypeName)))
            {
                var category = _categoryService.GetById(listEntryCategory.Id, domain.CategoryResponseGroup.Info);
                if (category.CatalogId != moveInfo.Catalog)
                {
                    category.CatalogId = moveInfo.Catalog;
                }
                if (category.ParentId != moveInfo.Category)
                {
                    category.ParentId = moveInfo.Category;
                }
                result.Add(category);
            }

            return result;
        }
    }
}
