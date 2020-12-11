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
            if (!string.IsNullOrEmpty(moveInfo.Category))
            {
                var targetCategory = (await _categoryService.GetByIdsAsync(new[] { moveInfo.Category }, CategoryResponseGroup.WithOutlines.ToString())).FirstOrDefault()
                    ?? throw new InvalidOperationException($"Destination category does not exist");

                foreach (var listEntryOfTypeCategory in moveInfo.ListEntries.Where(listEntry => listEntry.Type.EqualsInvariant(CategoryListEntry.TypeName)))
                {
                    var movedCategoryOutline = string.Join('\\', listEntryOfTypeCategory.Outline);

                    // TODO: Need to think on preventing cycles in the links, and add test
                    if (targetCategory.Outline.StartsWith(movedCategoryOutline, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("Cannot move category under itself");
                    }
                }
            }

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
    }
}
