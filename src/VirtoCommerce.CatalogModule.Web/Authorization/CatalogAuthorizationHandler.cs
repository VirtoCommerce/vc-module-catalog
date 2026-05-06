using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CatalogModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;
using Permissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    public sealed class CatalogAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        : PermissionAuthorizationHandlerBase<CatalogAuthorizationRequirement>
    {
        private static readonly Dictionary<string, string> _fallbackPermissions = new()
        {
            [Permissions.CategoriesCreate] = Permissions.Create,
            [Permissions.CategoriesRead] = Permissions.Read,
            [Permissions.CategoriesUpdate] = Permissions.Update,
            [Permissions.CategoriesDelete] = Permissions.Delete,
            [Permissions.ProductsCreate] = Permissions.Create,
            [Permissions.ProductsRead] = Permissions.Read,
            [Permissions.ProductsUpdate] = Permissions.Update,
            [Permissions.ProductsDelete] = Permissions.Delete,
        };

        private readonly MvcNewtonsoftJsonOptions _jsonOptions = jsonOptions.Value;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CatalogAuthorizationRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                TryAuthorizeWithPermission(context, requirement, requirement.Permission);
            }

            if (!context.HasSucceeded && _fallbackPermissions.TryGetValue(requirement.Permission, out var fallbackPermission))
            {
                TryAuthorizeWithPermission(context, requirement, fallbackPermission);
            }
        }

        private void TryAuthorizeWithPermission(AuthorizationHandlerContext context, CatalogAuthorizationRequirement requirement, string permission)
        {
            var userPermissions = context.User.FindPermissions(permission, _jsonOptions.SerializerSettings);
            if (userPermissions.Count == 0)
            {
                return;
            }

            var hasGlobalScope = userPermissions.Any(x => !x.AssignedScopes.OfType<SelectedCatalogScope>().Any());
            if (hasGlobalScope)
            {
                context.Succeed(requirement);
                return;
            }

            TryAuthorizeWithScopedPermission(context, requirement, permission);
        }

        private void TryAuthorizeWithScopedPermission(AuthorizationHandlerContext context, CatalogAuthorizationRequirement requirement, string permission)
        {
            var userPermissions = context.User.FindPermissions(permission, _jsonOptions.SerializerSettings);
            if (userPermissions.Count == 0)
            {
                return;
            }

            var allowedCatalogIdsList = new List<string>();

            foreach (var userPermission in userPermissions)
            {
                allowedCatalogIdsList.AddRange(userPermission.AssignedScopes.OfType<SelectedCatalogScope>().Select(x => x.CatalogId));
            }

            var allowedCatalogIds = allowedCatalogIdsList.Distinct().ToArray();

            switch (context.Resource)
            {
                case CatalogSearchCriteria catalogSearchCriteria:
                    if (CatalogScopeAuthorizationHelper.TryGetAuthorizedCatalogIds(catalogSearchCriteria.CatalogIds, allowedCatalogIds, out var catalogIds))
                    {
                        catalogSearchCriteria.CatalogIds = catalogIds;
                        context.Succeed(requirement);
                    }
                    break;
                case CatalogListEntrySearchCriteria listEntrySearchCriteria:
                    if (CatalogScopeAuthorizationHelper.TryGetAuthorizedCatalogIds(listEntrySearchCriteria.CatalogIds, allowedCatalogIds, out catalogIds))
                    {
                        listEntrySearchCriteria.CatalogIds = catalogIds;
                        context.Succeed(requirement);
                    }
                    break;
                case Catalog catalog when !catalog.IsTransient():
                    {
                        if (allowedCatalogIds.Contains(catalog.Id))
                        {
                            context.Succeed(requirement);
                        }

                        break;
                    }
                case IEnumerable<IHasCatalogId> hasCatalogIds:
                    {
                        var resourceCatalogIds = hasCatalogIds.Select(x => x.CatalogId).Distinct().ToList();
                        if (resourceCatalogIds.Intersect(allowedCatalogIds).Count() == resourceCatalogIds.Count)
                        {
                            context.Succeed(requirement);
                        }

                        break;
                    }
                case IHasCatalogId hasCatalogId:
                    {
                        if (allowedCatalogIds.Contains(hasCatalogId.CatalogId))
                        {
                            context.Succeed(requirement);
                        }

                        break;
                    }
                case ProductExportDataQuery dataQuery:
                    {
                        if (CatalogScopeAuthorizationHelper.TryGetAuthorizedCatalogIds(dataQuery.CatalogIds, allowedCatalogIds, out catalogIds))
                        {
                            dataQuery.CatalogIds = catalogIds;
                            context.Succeed(requirement);
                        }
                        break;
                    }
                case PropertyDictionaryItemSearchCriteria propertyDictionaryItemSearchCriteria:
                    {
                        if (CatalogScopeAuthorizationHelper.TryGetAuthorizedCatalogIds(propertyDictionaryItemSearchCriteria.CatalogIds, allowedCatalogIds, out catalogIds))
                        {
                            propertyDictionaryItemSearchCriteria.CatalogIds = catalogIds;
                            context.Succeed(requirement);
                        }
                        break;
                    }
                case IEnumerable<PropertyDictionaryItem>:
                    context.Succeed(requirement);
                    break;
            }
        }
    }
}
