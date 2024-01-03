using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Data.Authorization
{
    public sealed class CatalogAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public CatalogAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }
}
