using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Data.Authorization
{
    public sealed class CatalogAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public string[] Permissions { get; set; }

        public CatalogAuthorizationRequirement(string permission)
            : base(permission)
        {
        }

        public CatalogAuthorizationRequirement(params string[] permissions)
            : base(permissions[0])
        {
            Permissions = permissions;
        }
    }
}
