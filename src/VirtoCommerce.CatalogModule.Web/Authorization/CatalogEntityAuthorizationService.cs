using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    public sealed class CatalogEntityAuthorizationService(IAuthorizationService authorizationService)
    {
        public async Task<IList<CatalogListEntrySearchCriteria>> GetAuthorizedListEntrySearchCriteriaByObjectTypeAsync(ClaimsPrincipal user, CatalogListEntrySearchCriteria criteria, string categoryPermission, string productPermission)
        {
            var authorizedCriteria = new List<CatalogListEntrySearchCriteria>();
            var authorizationTargets = new[]
            {
                (ObjectType: nameof(Category), Permission: categoryPermission),
                (ObjectType: nameof(CatalogProduct), Permission: productPermission),
            };

            foreach (var target in authorizationTargets.Where(t => IsListEntryObjectTypeRequested(criteria.ObjectTypes, t.ObjectType)))
            {
                var typedCriteria = CreateTypedCriteria(criteria, target.ObjectType);
                if (await IsAuthorizedAsync(user, typedCriteria, target.Permission))
                {
                    authorizedCriteria.Add(typedCriteria);
                }
            }

            return authorizedCriteria;
        }

        public async Task<bool> TryAuthorizeEntitiesByTypeAsync<T>(ClaimsPrincipal user, IList<T> entities, string categoryPermission, string productPermission)
        {
            var catalogEntities = entities ?? [];
            var authorizationTargets = new[]
            {
                (Entities: catalogEntities.OfType<Category>().Cast<IHasCatalogId>().ToArray(), Permission: categoryPermission),
                (Entities: catalogEntities.OfType<CatalogProduct>().Cast<IHasCatalogId>().ToArray(), Permission: productPermission),
            };

            foreach (var target in authorizationTargets)
            {
                if (!target.Entities.IsNullOrEmpty() && !await IsAuthorizedAsync(user, target.Entities, target.Permission))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> TryAuthorizeMoveRequestByTypeAsync(ClaimsPrincipal user, ListEntriesMoveRequest moveRequest, IList<IEntity> sourceEntities, string categoryPermission, string productPermission)
        {
            var listEntries = moveRequest.ListEntries?.ToArray() ?? [];
            var entities = sourceEntities ?? [];
            var authorizationTargets = new[]
            {
                (ListEntryType: CategoryListEntry.TypeName, Permission: categoryPermission),
                (ListEntryType: ProductListEntry.TypeName, Permission: productPermission),
            };

            if (!await TryAuthorizeEntitiesByTypeAsync(user, entities, categoryPermission, productPermission))
            {
                return false;
            }

            foreach (var target in authorizationTargets)
            {
                if (listEntries.Any(x => x.Type.EqualsIgnoreCase(target.ListEntryType)) && !await IsAuthorizedAsync(user, moveRequest, target.Permission))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, object resource, string permission)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(user, resource, new CatalogAuthorizationRequirement(permission));
            return authorizationResult.Succeeded;
        }

        private static CatalogListEntrySearchCriteria CreateTypedCriteria(CatalogListEntrySearchCriteria criteria, string objectType)
        {
            var typedCriteria = criteria.CloneTyped();
            typedCriteria.ObjectTypes = [objectType];
            return typedCriteria;
        }

        private static bool IsListEntryObjectTypeRequested(IList<string> objectTypes, string objectType)
        {
            return objectTypes.IsNullOrEmpty() || objectTypes.Any(x => x.EqualsIgnoreCase(objectType));
        }

    }
}
