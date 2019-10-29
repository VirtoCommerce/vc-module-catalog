using System.Collections.Generic;
using System.Linq;

using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

using VC = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryMover : ListEntryMover<VC.Category>
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryMover"/> class.
        /// </summary>
        /// <param name="categoryService">
        /// The category service.
        /// </param>
        public CategoryMover(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public override void ConfirmMove(IEnumerable<VC.Category> entities)
        {
            _categoryService.Update(entities.ToArray());
        }

        public override List<VC.Category> PrepareMove(MoveContext moveContext)
        {
            var result = new List<VC.Category>();

            foreach (var listEntryCategory in moveContext.ListEntries.Where(
                listEntry => listEntry.Type.EqualsInvariant(ListEntryCategory.TypeName)))
            {
                var category = _categoryService.GetById(listEntryCategory.Id, VC.CategoryResponseGroup.Info);
                if (category.CatalogId != moveContext.Catalog)
                {
                    category.CatalogId = moveContext.Catalog;
                }

                if (category.ParentId != moveContext.Category)
                {
                    category.ParentId = moveContext.Category;
                }

                result.Add(category);
            }

            return result;
        }
    }
}
