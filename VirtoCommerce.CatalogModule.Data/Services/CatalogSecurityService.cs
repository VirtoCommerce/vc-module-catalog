using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogSecurityService : ICatalogSecurityService
    {
        private readonly ISecurityService _securityService;
        private readonly ICategoryService _categoryService;
        private readonly IPermissionScopeService _permissionScopeService;

        /// <summary>
        /// Used to replace SearchCriteria CatalogId or CategoryId to prevent user access to not allowed scope
        /// </summary>
        private const string RestrictedId = "Restricted";

        public CatalogSecurityService(ISecurityService securityService, ICategoryService categoryService,
            IPermissionScopeService permissionScopeService)
        {
            _securityService = securityService;
            _categoryService = categoryService;
            _permissionScopeService = permissionScopeService;
        }

        #region ISearchCreteriaSecurity

        public void ApplyUserRestrictions(SearchCriteria criteria, string userName)
        {
            var catalogReadPermissionScopes = _securityService.GetUserPermissions(userName)
                                                   .Where(x => x.Id.StartsWith(CatalogPredefinedPermissions.Read))
                                                   .SelectMany(x => x.AssignedScopes)
                                                   .ToArray();

            if (!catalogReadPermissionScopes.IsNullOrEmpty())
            {
                var restrictions = GetCatalogAndCategoryRestrictions(catalogReadPermissionScopes);

                var allAllowedCatalogIds = restrictions.Select(x => x.Key).ToArray();
                var allAllowedCategoryIds = restrictions.SelectMany(x => x.Value).ToArray();

                var catalogId = FilterCatalogIds(new[] { criteria.CatalogId }, allAllowedCatalogIds).FirstOrDefault();
                var catalogIds = FilterCatalogIds(criteria.CatalogIds, allAllowedCatalogIds);

                if (!string.IsNullOrWhiteSpace(catalogId))
                {
                    allAllowedCategoryIds = restrictions.Where(x => x.Key.EqualsInvariant(catalogId))
                        .SelectMany(x => x.Value)
                        .ToArray();
                }

                if (!catalogIds.IsNullOrEmpty())
                {
                    allAllowedCategoryIds = restrictions.Where(x => catalogIds.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
                        .SelectMany(x => x.Value)
                        .ToArray();
                }

                string categoryId = null;
                if (catalogId != null && !string.IsNullOrWhiteSpace(criteria.CatalogId))
                {
                    categoryId = FilterCategoryIds(new[] { criteria.CategoryId }, allAllowedCategoryIds, allAllowedCatalogIds).FirstOrDefault();
                }

                var categoryIds = Array.Empty<string>();
                if (!catalogIds.IsNullOrEmpty() && !criteria.CatalogIds.IsNullOrEmpty())
                {
                    categoryIds = FilterCategoryIds(criteria.CategoryIds, allAllowedCategoryIds, allAllowedCatalogIds);
                }

                criteria.CatalogId = catalogId;
                criteria.CatalogIds = catalogIds;
                criteria.CategoryId = categoryId;
                criteria.CategoryIds = categoryIds;
            }
        }

        public bool UserHasPermissionForObjects(string permission, string userName, params object[] objects)
        {
            var scopes = objects.SelectMany(x => _permissionScopeService.GetObjectPermissionScopeStrings(x)).Distinct().ToArray();
            return _securityService.UserHasAnyPermission(userName, scopes, permission);
        }

        public string[] GetObjectPermissionScopeStrings(object obj)
        {
            return _permissionScopeService.GetObjectPermissionScopeStrings(obj).ToArray();
        }

        #endregion

        private string[] FilterCategoryIds(string[] categoryIds, string[] allowedCategoryIds, string[] allowedCatalogIds)
        {
            var result = Array.Empty<string>();

            if (categoryIds.IsNullOrEmpty())
            {
                result = allowedCategoryIds;
            }

            if (allowedCategoryIds.IsNullOrEmpty())
            {
                result = categoryIds;
            }

            if (!categoryIds.IsNullOrEmpty() && !allowedCategoryIds.IsNullOrEmpty())
            {
                var categories = _categoryService.GetByIds(categoryIds, CategoryResponseGroup.WithParents);
                var ids = categories.Where(x =>
                        x.Parents.Any(p => allowedCategoryIds.Contains(p.Id, StringComparer.OrdinalIgnoreCase))
                        && allowedCatalogIds.Contains(x.CatalogId, StringComparer.OrdinalIgnoreCase))
                    .Select(x => x.Id)
                    .ToArray();

                // add unexisted id to prevent access to restricted Entity
                result = ids.Any() ? ids : new[] { RestrictedId };
            }

            return result;
        }

        private string[] FilterCatalogIds(string[] catalogIds, string[] allowedCatalogIds)
        {
            var result = Array.Empty<string>();

            if (catalogIds.IsNullOrEmpty())
            {
                result = allowedCatalogIds;
            }

            if (allowedCatalogIds.IsNullOrEmpty())
            {
                result = catalogIds;
            }

            if (!catalogIds.IsNullOrEmpty() && !allowedCatalogIds.IsNullOrEmpty())
            {
                var ids = allowedCatalogIds.Where(x => catalogIds.Contains(x, StringComparer.OrdinalIgnoreCase))
                    .ToArray();

                // add unexisted id to prevent access to restricted Entity
                result = ids.Any() ? ids : new[] { RestrictedId };
            }

            return result;
        }

        /// <summary>
        /// Return Catalog and Category restriction dictionary
        /// </summary>
        /// <param name="permissionScopes">All user permission scopes</param>
        /// <returns>
        /// Key - allowed catalog Id, Value - array of allowed category ids in this catalog.
        /// Empty Value meens, that user can access any category in this catalog.
        /// </returns>
        private Dictionary<string, string[]> GetCatalogAndCategoryRestrictions(PermissionScope[] permissionScopes)
        {
            var restrictedUserCategoryIds = FilterScopes<CatalogSelectedCategoryScope>(permissionScopes);
            var restrictedUserCatalogIds = FilterScopes<CatalogSelectedScope>(permissionScopes);

            var categories = _categoryService.GetByIds(restrictedUserCategoryIds, CategoryResponseGroup.None);
            var result = categories.Where(x => restrictedUserCatalogIds.Any(c => c == x.CatalogId))
                                         .GroupBy(x => x.CatalogId)
                                         .ToDictionary(x => x.Key, x => x.Select(y => y.Id)
                                         .ToArray());

            var fullyAllowedCatalogIds = restrictedUserCatalogIds.Except(result.Keys).ToArray();

            if (fullyAllowedCatalogIds.Any())
            {
                result.AddRange(fullyAllowedCatalogIds.ToDictionary(x => x, x => Array.Empty<string>()));
            }

            if (restrictedUserCategoryIds.Any() && !restrictedUserCatalogIds.Any())
            {
                result.Add(RestrictedId, new[] { RestrictedId });
            }

            return result;
        }

        private string[] FilterScopes<T>(PermissionScope[] permissionScopes) where T : PermissionScope
        {
            var result = Array.Empty<string>();
            var scopes = permissionScopes.OfType<T>().ToArray();

            if (scopes.Any())
            {
                result = scopes.Select(x => x.Scope)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            return result;
        }

    }
}
