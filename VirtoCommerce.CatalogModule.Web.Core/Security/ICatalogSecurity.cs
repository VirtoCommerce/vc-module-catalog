using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Security
{
    public interface ICatalogSecurity
    {
        void ApplayUserRestrictions(SearchCriteria criteria, string userName);
        bool UserHasPermissionForObjects(string permission, string userName, params object[] objects);
        string[] GetObjectPermissionScopeStrings(object obj);
    }
}
