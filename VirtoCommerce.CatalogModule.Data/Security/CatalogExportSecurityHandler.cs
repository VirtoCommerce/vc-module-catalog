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
                    var catalogScopes = readPermissionScopes.OfType<CatalogSelectedScope>().ToList();
                    if (catalogScopes.Any())
                    {
                        productExportDataQuery.CatalogIds = ApplyScope(productExportDataQuery.CatalogIds, catalogScopes);
                    }

                    var categoryScopes = readPermissionScopes.OfType<CatalogSelectedCategoryScope>().ToList();
                    if (categoryScopes.Any())
                    {
                        productExportDataQuery.CategoryIds = ApplyScope(productExportDataQuery.CategoryIds, categoryScopes);
                    }
                }

                if (dataQuery is CatalogFullExportDataQuery catalogFullExportDataQuery)
                {
                    var catalogScopes = readPermissionScopes.OfType<CatalogSelectedScope>().ToList();

                    if (catalogScopes.Any())
                    {
                        catalogFullExportDataQuery.CatalogIds = ApplyScope(catalogFullExportDataQuery.CatalogIds, catalogScopes);
                    }
                }
            }

            return result;
        }


        private string[] ApplyScope(string[] target, IEnumerable<PermissionScope> permissionScope)
        {
            var allowedIds = permissionScope.Select(x => x.Scope)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            return target.IsNullOrEmpty() ? allowedIds : target.Intersect(allowedIds).ToArray();

        }
    }
}
