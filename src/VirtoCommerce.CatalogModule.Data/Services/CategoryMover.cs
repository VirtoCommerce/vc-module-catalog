using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Validation;
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
                var category = await _categoryService.GetByIdAsync(listEntryCategory.Id, CategoryResponseGroup.Info.ToString());
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
            if (moveInfo == null)
            {
                throw new ArgumentNullException(nameof(moveInfo));
            }

            var validator = new ListEntriesMoveRequestValidator(_categoryService);
            await validator.ValidateAndThrowAsync(moveInfo);
        }
    }
}
