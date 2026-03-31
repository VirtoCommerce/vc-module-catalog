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
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    public sealed class CatalogAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        : PermissionAuthorizationHandlerBase<CatalogAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions = jsonOptions.Value;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CatalogAuthorizationRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                TryAuthorizeWithScopedPermission(context, requirement, requirement.Permission);
            }

            if (!context.HasSucceeded && !string.IsNullOrEmpty(requirement.FallbackPermission))
            {
                TryAuthorizeWithPermission(context, requirement, requirement.FallbackPermission);
            }
        }

        private void TryAuthorizeWithPermission(AuthorizationHandlerContext context, CatalogAuthorizationRequirement requirement, string permission)
        {
            var userPermissions = context.User.FindPermissions(permission, _jsonOptions.SerializerSettings);
            if (userPermissions.Count == 0)
            {
                return;
            }

            var hasGlobalScope = userPermissions.Any(p => !p.AssignedScopes.OfType<SelectedCatalogScope>().Any());
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

            if (userPermissions.Count <= 0)
            {
                return;
            }

            var allowedCatalogIdsList = new List<string>();

            foreach (var userPermission in userPermissions)
            {
                allowedCatalogIdsList.AddRange(userPermission.AssignedScopes.OfType<SelectedCatalogScope>().Select(y => y.CatalogId));
            }

            var allowedCatalogIds = allowedCatalogIdsList.Distinct().ToArray();

            switch (context.Resource)
            {
                case CatalogSearchCriteria catalogSearchCriteria:
                    catalogSearchCriteria.CatalogIds = catalogSearchCriteria.CatalogIds?.Any() ?? false
                        ? catalogSearchCriteria.CatalogIds.Where(x => allowedCatalogIds.Contains(x)).ToArray()
                        : allowedCatalogIds;

                    context.Succeed(requirement);
                    break;
                case CatalogListEntrySearchCriteria listEntrySearchCriteria:
                    listEntrySearchCriteria.CatalogIds = listEntrySearchCriteria.CatalogIds?.Any() ?? false
                        ? listEntrySearchCriteria.CatalogIds.Where(x => allowedCatalogIds.Contains(x)).ToArray()
                        : allowedCatalogIds;

                    context.Succeed(requirement);
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
                        var catalogIds = hasCatalogIds.Select(x => x.CatalogId).Distinct().ToList();
                        if (catalogIds.Intersect(allowedCatalogIds).Count() == catalogIds.Count)
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
                        dataQuery.CatalogIds = dataQuery.CatalogIds.IsNullOrEmpty() ? allowedCatalogIds : dataQuery.CatalogIds.Intersect(allowedCatalogIds).ToArray();
                        context.Succeed(requirement);
                        break;
                    }
                case PropertyDictionaryItemSearchCriteria propertyDictionaryItemSearchCriteria:
                    {
                        propertyDictionaryItemSearchCriteria.CatalogIds = propertyDictionaryItemSearchCriteria.CatalogIds.IsNullOrEmpty() ? allowedCatalogIds : propertyDictionaryItemSearchCriteria.CatalogIds.Intersect(allowedCatalogIds).ToArray();
                        context.Succeed(requirement);
                        break;
                    }
                case IEnumerable<PropertyDictionaryItem>:
                    context.Succeed(requirement);
                    break;
            }
        }
    }
}
