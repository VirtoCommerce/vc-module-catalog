using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Services.Validation
{
    public class MoveInfoValidator : AbstractValidator<MoveInfo>
    {
        private readonly ICategoryService _categoryService;

        public MoveInfoValidator(ICategoryService categoryService)
        {
            _categoryService = categoryService;

            RuleFor(x => x).Custom((moveInfo, context) =>
            {
                if (string.IsNullOrEmpty(moveInfo.Category))
                {
                    return;
                }

                var targetCategory = _categoryService.GetByIds(new[] { moveInfo.Category }, CategoryResponseGroup.WithParents).FirstOrDefault();

                if (targetCategory == null)
                {
                    context.AddFailure($"Destination category does not exist.");
                    return;
                }

                foreach (var movedCategory in moveInfo.ListEntries.Where(x => x.Type.EqualsInvariant(ListEntryCategory.TypeName)))
                {
                    var movedCategoryParts = new List<string>() { movedCategory.CatalogId };

                    if (!movedCategory.Outline.IsNullOrEmpty())
                    {
                        movedCategoryParts.AddRange(movedCategory.Outline);
                    }

                    movedCategoryParts.Add(movedCategory.Id);

                    var movedCategoryPath = string.Join("/", movedCategoryParts);
                    var targetCategoryPath = GetCategoryOutline(targetCategory);
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

        private string GetCategoryOutline(Domain.Catalog.Model.Category category)
        {
            // catalogId + categoriesPaths + ownCategoryId
            var parts = new List<string>() { category.CatalogId };

            if (!category.Parents.IsNullOrEmpty())
            {
                parts.AddRange(category.Parents?.Select(x => x.Id));
            }
            parts.Add(category.Id);

            return string.Join("/", parts);
        }
    }

}
