using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using domain = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
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
                var targetCategory = _categoryService.GetById(moveInfo.Category, domain.CategoryResponseGroup.WithOutlines);

                if (category.Id == moveInfo.Category)
                {
                    throw new ArgumentException("Unable to move category to itself");
                }

                var outlinesIds = targetCategory.Outlines.SelectMany(x => x.Items);

                if (outlinesIds.Any(x => x.Id.EqualsInvariant(category.Id)))
                {
                    throw new ArgumentException("Unable to move category to its descendant");
                }

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
