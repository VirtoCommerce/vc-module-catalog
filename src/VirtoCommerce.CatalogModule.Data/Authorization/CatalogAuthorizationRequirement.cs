using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Data.Authorization
{
    public sealed class CatalogAuthorizationRequirement(string permission, string fallbackPermission = null)
        : PermissionAuthorizationRequirement(permission)
    {
        public string FallbackPermission { get; } = fallbackPermission;
    }
}
