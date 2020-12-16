using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class ListEntriesMoveRequestValidator : AbstractValidator<ListEntriesMoveRequest>
    {
        private readonly ICategoryService _categoryService;

        public ListEntriesMoveRequestValidator(ICategoryService categoryService)
        {
            _categoryService = categoryService;

            RuleFor(x => x).CustomAsync(async (moveInfo, context, token) =>
            {
                if (string.IsNullOrEmpty(moveInfo.Category))
                {
                    return;
                }

                var targetCategory = (await _categoryService.GetByIdsAsync(new[] { moveInfo.Category }, CategoryResponseGroup.WithOutlines.ToString())).FirstOrDefault();

                if (targetCategory == null)
                {
                    context.AddFailure($"Destination category does not exist.");
                    return;
                }

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
                        context.AddFailure($"Cannot move category under itself.");
                        return;
                    }
                }
            });
        }
    }
}
