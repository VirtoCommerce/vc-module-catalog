using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryMover : ListEntryMover<Category>
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

        public override Task ConfirmMoveAsync(IEnumerable<Category> entities)
        {
            return _categoryService.SaveChangesAsync(entities.ToArray());
        }

        public override async Task<List<Category>> PrepareMoveAsync(ListEntriesMoveRequest moveInfo)
        {
            await ValidateOperationArguments(moveInfo);

            var result = new List<Category>();

            foreach (var listEntryCategory in moveInfo.ListEntries.Where(
                listEntry => listEntry.Type.EqualsInvariant(CategoryListEntry.TypeName)))
            {
                var category = (await _categoryService.GetByIdsAsync(new[] { listEntryCategory.Id }, CategoryResponseGroup.Info.ToString())).FirstOrDefault();
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

        protected virtual async Task ValidateOperationArguments(ListEntriesMoveRequest moveInfo)
        {
            if (!string.IsNullOrEmpty(moveInfo.Category))
            {
                var targetCategory = (await _categoryService.GetByIdsAsync(new[] { moveInfo.Category }, CategoryResponseGroup.WithOutlines.ToString())).FirstOrDefault()
                    ?? throw new InvalidOperationException($"Destination category does not exist.");

                foreach (var movedCategory in moveInfo.ListEntries.Where(x => x.Type.EqualsInvariant(CategoryListEntry.TypeName)))
                {
                    var movedCategoryPath = string.Join("/", movedCategory.Outline);
                    var targetCategoryPath = string.Join("/", targetCategory.Outlines.FirstOrDefault());
                    // Here we comparing that category will not be placed under itself.
                    // E.g. we have hierarchy: Catalog1\Cat1\Cat2 - We should not allow to move Cat1 under Cat2.
                    // Target category path - Catalog1\Cat1\Cat2, moved category path - Catalog1\Cat1.
                    // Target category path should not be part of moved category full physical path.
                    // Because if moved category is a parent of a target one, it should be in target category path.                    
                    if (targetCategoryPath.EqualsInvariant(movedCategoryPath)
                        || targetCategoryPath.StartsWith($"{movedCategoryPath}/"))
                    {
                        throw new InvalidOperationException("Cannot move category under itself");
                    }
                }
            }
        }
    }
}
