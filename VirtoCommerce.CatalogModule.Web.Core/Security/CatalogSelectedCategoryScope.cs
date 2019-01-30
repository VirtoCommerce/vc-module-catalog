using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogModule.Web.Security
{
    public class CatalogSelectedCategoryScope : PermissionScope
    {
        private readonly ICategoryService _categoryService;

        public CatalogSelectedCategoryScope(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public override bool IsScopeAvailableForPermission(string permission)
        {
            return permission == CatalogPredefinedPermissions.Read
                      || permission == CatalogPredefinedPermissions.Update;
        }

        public override IEnumerable<string> GetEntityScopeStrings(object entity)
        {
            if (entity == null) { throw new ArgumentNullException(nameof(entity)); }

            var categoryId = GetCategoryIdFromEntity(entity);

            if (categoryId != null)
            {
                var resultCategory = _categoryService.GetById(categoryId, CategoryResponseGroup.WithParents);
                if (resultCategory != null)
                {
                    //Need to return scopes for all parent categories to support scope inheritance (permission defined on parent category should be active for children)
                    var retVal = new[] { resultCategory.Id }.Concat(resultCategory.Parents.Select(x => x.Id)).Select(x => $"{Type}:{x}");
                    return retVal;
                }
            }

            return Enumerable.Empty<string>();
        }

        private string GetCategoryIdFromEntity(object entity)
        {
            switch (entity)
            {
                case Category category:
                    return category.Id;
                case CatalogProduct product:
                    return product.CategoryId;
                case Model.ListEntryLink link:
                    return link.CategoryId;
                case Model.Property property:
                    return property.CategoryId;
            }

            return null;
        }
    }
}
