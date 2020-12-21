using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogModule.Data.Services.Validation;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryMover : ListEntryMover<domain.Category>
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

        public override void ConfirmMove(IEnumerable<domain.Category> entities)
        {
            _categoryService.Update(entities.ToArray());
        }

        public override List<domain.Category> PrepareMove(MoveInfo moveInfo)
        {
            ValidateOperationArguments(moveInfo);

            var result = new List<domain.Category>();

            foreach (var listEntryCategory in moveInfo.ListEntries.Where(
                listEntry => listEntry.Type.EqualsInvariant(ListEntryCategory.TypeName)))
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


        protected virtual void ValidateOperationArguments(MoveInfo moveInfo)
        {
            if (moveInfo == null)
            {
                throw new ArgumentNullException(nameof(moveInfo));
            }

            var validator = new MoveInfoValidator(_categoryService);
            validator.ValidateAndThrow(moveInfo);
        }
    }
}
