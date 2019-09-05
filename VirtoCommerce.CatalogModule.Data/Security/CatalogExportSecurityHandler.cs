using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Security;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogModule.Data.Security
{
    public class CatalogExportSecurityHandler : IExportSecurityHandler
    {
        private readonly ISecurityService _securityService;

        public CatalogExportSecurityHandler(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        public bool Authorize(string userName, ExportDataQuery dataQuery)
        {
            var result = true;

            if (!_securityService.UserHasAnyPermission(userName, null, CatalogPredefinedPermissions.Read))
            {

                // Get user 'read' permission scopes
                var readPermissionScopes = _securityService.GetUserPermissions(userName)
                    .Where(x => x.Id.StartsWith(CatalogPredefinedPermissions.Read))
                    .SelectMany(x => x.AssignedScopes)
                    .ToList();

                if (dataQuery is ProductExportDataQuery productExportDataQuery)
                {
                    productExportDataQuery.CatalogIds = ApplyScope<CatalogSelectedScope>(productExportDataQuery.CatalogIds, readPermissionScopes);
                    productExportDataQuery.CategoryIds = ApplyScope<CatalogSelectedCategoryScope>(productExportDataQuery.CategoryIds, readPermissionScopes);
                }

                if (dataQuery is CatalogFullExportDataQuery catalogFullExportDataQuery)
                {
                    catalogFullExportDataQuery.CatalogIds = ApplyScope<CatalogSelectedScope>(catalogFullExportDataQuery.CatalogIds, readPermissionScopes);
                }
            }

            return result;
        }

        private string[] ApplyScope<TScopeType>(string[] target, IEnumerable<PermissionScope> permissionScopes) where TScopeType : PermissionScope
        {
            var result = Array.Empty<string>();
            var typedScopes = permissionScopes.OfType<TScopeType>().ToList();

            if (typedScopes.Any())
            {
                var allowedIds = typedScopes.Select(x => x.Scope)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                result = target.IsNullOrEmpty() ? allowedIds : target.Intersect(allowedIds).ToArray();

            }

            return result;
        }
    }
}
