using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogModule.Data.Services.Security
{
    public class CatalogSecurity : ICatalogSecurity
    {
        private readonly ISecurityService _securityService;
        private readonly ICategoryService _categoryService;
        private readonly IPermissionScopeService _permissionScopeService;


        public CatalogSecurity(ISecurityService securityService, ICategoryService categoryService,
            IPermissionScopeService permissionScopeService)
        {
            _securityService = securityService;
            _categoryService = categoryService;
            _permissionScopeService = permissionScopeService;
        }

        #region ISearchCreteriaSecurity

        public void ApplayUserRestrictions(SearchCriteria criteria, string userName)
        {
            var permissionScopes = GetReedPermissionScopes(userName);
            if (permissionScopes.IsNullOrEmpty())
            {
                return; // allowed all
            }

            var restrictions = GetCatalogAndCategiryRestrictions(permissionScopes);

            var allAllowedCatalogIds = restrictions.Select(x => x.Key).ToArray();
            var allAllowedCategoryIds = restrictions.SelectMany(x => x.Value).ToArray();

            var catalogId = FilterCatalogIds(new[] { criteria.CatalogId }, allAllowedCatalogIds).FirstOrDefault();
            var catalogIds = FilterCatalogIds(criteria.CatalogIds, allAllowedCatalogIds);

            if (!string.IsNullOrWhiteSpace(catalogId))
            {
                allAllowedCategoryIds = restrictions.Where(x => x.Key == catalogId)
                                                    .SelectMany(x => x.Value)
                                                    .ToArray();
            }

            if (!catalogIds.IsNullOrEmpty())
            {
                allAllowedCategoryIds = restrictions.Where(x => catalogIds.Contains(x.Key))
                                                    .SelectMany(x => x.Value)
                                                    .ToArray();
            }

            string categoryId = null;
            if (catalogId != null && !string.IsNullOrWhiteSpace(criteria.CatalogId))
            {
                categoryId = FilterCategoryIds(new[] { criteria.CategoryId }, allAllowedCategoryIds).FirstOrDefault();
            }

            var categoryIds = new string[0];
            if (!catalogIds.IsNullOrEmpty() && !criteria.CatalogIds.IsNullOrEmpty())
            {
                categoryIds = FilterCategoryIds(criteria.CategoryIds, allAllowedCategoryIds);
            }

            criteria.CatalogId = catalogId;
            criteria.CatalogIds = catalogIds;
            criteria.CategoryId = categoryId;
            criteria.CategoryIds = categoryIds;
        }

        public bool UserHasPermissionForObjects(string permission, string userName, params object[] objects)
        {
            //Scope bound security check
            var scopes = objects.SelectMany(x => _permissionScopeService.GetObjectPermissionScopeStrings(x)).Distinct().ToArray();
            return _securityService.UserHasAnyPermission(userName, scopes, permission);
        }

        public string[] GetObjectPermissionScopeStrings(object obj)
        {
            return _permissionScopeService.GetObjectPermissionScopeStrings(obj).ToArray();
        }

        #endregion

        private string[] FilterCategoryIds(string[] categoryIds, string[] allowedCategoryIds)
        {
            if (categoryIds.IsNullOrEmpty()) return allowedCategoryIds;
            if (allowedCategoryIds.IsNullOrEmpty()) return categoryIds;

            var categories = _categoryService.GetByIds(categoryIds, CategoryResponseGroup.WithParents);
            var ids = categories.Where(x => x.Parents.Any(p => allowedCategoryIds.Contains(p.Id)))
                                .Select(x => x.Id)
                                .ToArray();

            return ids;
        }

        private string[] FilterCatalogIds(string[] catalogIds, string[] allowedCatalogIds)
        {
            if (catalogIds.IsNullOrEmpty()) return allowedCatalogIds;
            if (allowedCatalogIds.IsNullOrEmpty()) return catalogIds;

            if (allowedCatalogIds.Any() && !catalogIds.IsNullOrEmpty())
            {
                return allowedCatalogIds.Where(x => catalogIds.Contains(x)).ToArray();
            }
            return catalogIds;
        }

        /// <summary>
        /// Return Catalog and Category restriction dictionary
        /// </summary>
        /// <returns>
        /// Key - allowed catalog Id, Value - array of allowed category ids in this catalog.
        /// Empty Value meens, that user can access any category in this catalog.
        /// </returns>
        private Dictionary<string, string[]> GetCatalogAndCategiryRestrictions(PermissionScope[] permissionScopes)
        {
            var restrictedUserCategoryIds = GetAllowedCategoryIds(permissionScopes);
            var restrictedUserCatalogIds = GetAllowedCatalogIds(permissionScopes);

            var categories = _categoryService.GetByIds(restrictedUserCategoryIds, CategoryResponseGroup.None);

            var restrictions = categories.Where(x => restrictedUserCatalogIds.Any(c => c == x.CatalogId))
                                         .GroupBy(x => x.CatalogId)
                                         .ToDictionary(x => x.Key, x => x.Select(y => y.Id)
                                         .ToArray());

            var fullyAllowedCatalogIds = restrictedUserCatalogIds.Except(restrictions.Keys).ToArray();
            if (fullyAllowedCatalogIds.Any())
            {
                restrictions.AddRange(fullyAllowedCatalogIds.ToDictionary(x => x, x => new string[0]));
            }

            return restrictions;
        }

        private string[] GetAllowedCatalogIds(PermissionScope[] permissionScopes)
        {
            var catalogScopes = permissionScopes.OfType<CatalogSelectedScope>();
            if (catalogScopes.Any())
            {
                return catalogScopes.Select(x => x.Scope)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            return new string[0];
        }

        private string[] GetAllowedCategoryIds(PermissionScope[] permissionScopes)
        {
            var categoryScopes = permissionScopes.OfType<CatalogSelectedCategoryScope>();
            if (categoryScopes.Any())
            {
                return categoryScopes.Select(x => x.Scope)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            return new string[0];
        }

        private PermissionScope[] GetReedPermissionScopes(string userName)
        {
            return _securityService.GetUserPermissions(userName)
                .Where(x => x.Id.StartsWith(CatalogPredefinedPermissions.Read))
                .SelectMany(x => x.AssignedScopes)
                .ToArray();
        }
    }
}
