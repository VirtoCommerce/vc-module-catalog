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
        public async Task<IList<CatalogListEntrySearchCriteria>> GetAuthorizedCriteriaByTypeAsync(ClaimsPrincipal user, CatalogListEntrySearchCriteria criteria, string categoryPermission, string productPermission, string fallbackPermission)
        {
            var authorizedCriteria = new List<CatalogListEntrySearchCriteria>();

            if (RequestsObjectType(criteria.ObjectTypes, nameof(Category)))
            {
                var categoryCriteria = CreateTypedCriteria(criteria, nameof(Category));
                if (await IsAuthorizedAsync(user, categoryCriteria, categoryPermission, fallbackPermission))
                {
                    authorizedCriteria.Add(categoryCriteria);
                }
            }

            if (RequestsObjectType(criteria.ObjectTypes, nameof(CatalogProduct)))
            {
                var productCriteria = CreateTypedCriteria(criteria, nameof(CatalogProduct));
                if (await IsAuthorizedAsync(user, productCriteria, productPermission, fallbackPermission))
                {
                    authorizedCriteria.Add(productCriteria);
                }
            }

            return authorizedCriteria;
        }

        public async Task<bool> TryAuthorizeEntitiesByTypeAsync(ClaimsPrincipal user, IEnumerable<IEntity> entities, string categoryPermission, string productPermission, string fallbackPermission)
        {
            var catalogEntities = entities?.ToArray() ?? [];

            var categories = catalogEntities.OfType<Category>().ToArray();
            if (!categories.IsNullOrEmpty() && !await IsAuthorizedAsync(user, categories, categoryPermission, fallbackPermission))
            {
                return false;
            }

            var products = catalogEntities.OfType<CatalogProduct>().ToArray();
            if (!products.IsNullOrEmpty() && !await IsAuthorizedAsync(user, products, productPermission, fallbackPermission))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> TryAuthorizeMoveRequestByTypeAsync(ClaimsPrincipal user, ListEntriesMoveRequest moveRequest, IEnumerable<IEntity> sourceEntities, string categoryPermission, string productPermission, string fallbackPermission)
        {
            var listEntries = moveRequest.ListEntries?.ToArray() ?? [];
            var entities = sourceEntities?.ToArray() ?? [];

            if (!await TryAuthorizeEntitiesByTypeAsync(user, entities, categoryPermission, productPermission, fallbackPermission))
            {
                return false;
            }

            if (listEntries.Any(x => IsCategoryType(x.Type)) &&
                !await IsAuthorizedAsync(user, moveRequest, categoryPermission, fallbackPermission))
            {
                return false;
            }

            if (listEntries.Any(x => IsProductType(x.Type)) &&
                !await IsAuthorizedAsync(user, moveRequest, productPermission, fallbackPermission))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, object resource, string permission, string fallbackPermission = null)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(user, resource, new CatalogAuthorizationRequirement(permission, fallbackPermission));
            return authorizationResult.Succeeded;
        }

        private static CatalogListEntrySearchCriteria CreateTypedCriteria(CatalogListEntrySearchCriteria criteria, string objectType)
        {
            var typedCriteria = criteria.CloneTyped();
            typedCriteria.ObjectTypes = [objectType];
            return typedCriteria;
        }

        private static bool RequestsObjectType(IEnumerable<string> objectTypes, string objectType)
        {
            return objectTypes.IsNullOrEmpty() || objectTypes.Any(x => x.EqualsIgnoreCase(objectType));
        }

        private static bool IsCategoryType(string objectType)
        {
            return objectType.EqualsIgnoreCase(CategoryListEntry.TypeName);
        }

        private static bool IsProductType(string objectType)
        {
            return objectType.EqualsIgnoreCase(ProductListEntry.TypeName);
        }
    }
}
