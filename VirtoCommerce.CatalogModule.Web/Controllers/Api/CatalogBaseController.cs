using System.Net;
using System.Web.Http;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    public class CatalogBaseController : ApiController
    {
        private readonly ICatalogSecurity _catalogSecurity;

        public CatalogBaseController(ICatalogSecurity catalogSecurity)
        {
            _catalogSecurity = catalogSecurity;
        }

        protected string[] GetObjectPermissionScopeStrings(object obj)
        {
            return _catalogSecurity.GetObjectPermissionScopeStrings(obj);
        }

        protected void CheckCurrentUserHasPermissionForObjects(string permission, params object[] objects)
        {
            var userName = User.Identity.Name;
            if (!_catalogSecurity.UserHasPermissionForObjects(permission, userName, objects))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        // <summary>
        // Filter catalog search criteria based on current user permissions
        // </summary>
        protected void ApplyRestrictionsForCurrentUser(SearchCriteria criteria)
        {
            var userName = User.Identity.Name;
            _catalogSecurity.ApplayUserRestrictions(criteria, userName);
        }
    }
}
