using System.Net;
using System.Web.Http;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    public class CatalogBaseController : ApiController
    {
        private readonly ICatalogSecurityService _catalogSecurityService;

        public CatalogBaseController(ICatalogSecurityService catalogSecurityService)
        {
            _catalogSecurityService = catalogSecurityService;
        }

        protected string[] GetObjectPermissionScopeStrings(object obj)
        {
            return _catalogSecurityService.GetObjectPermissionScopeStrings(obj);
        }

        protected void CheckCurrentUserHasPermissionForObjects(string permission, params object[] objects)
        {
            var userName = User.Identity.Name;
            if (!_catalogSecurityService.UserHasPermissionForObjects(permission, userName, objects))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Filter catalog search criteria based on current user permissions
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected void ApplyRestrictionsForCurrentUser(SearchCriteria criteria)
        {
            var userName = User.Identity.Name;
            _catalogSecurityService.ApplyUserRestrictions(criteria, userName);
        }
    }
}
