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
    public sealed class CatalogAuthorizationHandler : PermissionAuthorizationHandlerBase<CatalogAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        public CatalogAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        {
            _jsonOptions = jsonOptions.Value;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CatalogAuthorizationRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                var userPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);
                if (userPermission != null)
                {
                    var allowedCatalogIds = userPermission.AssignedScopes.OfType<SelectedCatalogScope>()
                                                          .Select(x => x.CatalogId)
                                                          .Distinct()
                                                          .ToArray();

                    if (context.Resource is CatalogSearchCriteria catalogSearchCriteria)
                    {
                        catalogSearchCriteria.CatalogIds = allowedCatalogIds;
                        context.Succeed(requirement);
                    }
                    else if (context.Resource is CatalogListEntrySearchCriteria listEntrySearchCriteria)
                    {
                        listEntrySearchCriteria.CatalogIds = allowedCatalogIds;
                        context.Succeed(requirement);
                    }
                    else if (context.Resource is Catalog catalog && !catalog.IsTransient())
                    {
                        if (allowedCatalogIds.Contains(catalog.Id))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is IEnumerable<IHasCatalogId> hasCatalogIds)
                    {
                        var catalogIds = hasCatalogIds.Select(x => x.CatalogId).Distinct().ToList();
                        if (catalogIds.Intersect(allowedCatalogIds).Count() == catalogIds.Count)
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is IHasCatalogId hasCatalogId)
                    {
                        if (allowedCatalogIds.Contains(hasCatalogId.CatalogId))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is ProductExportDataQuery dataQuery)
                    {
                        if (dataQuery.CatalogIds.IsNullOrEmpty())
                        {
                            dataQuery.CatalogIds = allowedCatalogIds;
                        }
                        else
                        {
                            dataQuery.CatalogIds = dataQuery.CatalogIds.Intersect(allowedCatalogIds).ToArray();
                        }
                        context.Succeed(requirement);
                    }
                    else if (context.Resource is PropertyDictionaryItemSearchCriteria propertyDictionaryItemSearchCriteria)
                    {
                        if (propertyDictionaryItemSearchCriteria.CatalogIds.IsNullOrEmpty())
                        {
                            propertyDictionaryItemSearchCriteria.CatalogIds = allowedCatalogIds;
                        }
                        else
                        {
                            propertyDictionaryItemSearchCriteria.CatalogIds = propertyDictionaryItemSearchCriteria.CatalogIds.Intersect(allowedCatalogIds).ToArray();
                        }
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}
