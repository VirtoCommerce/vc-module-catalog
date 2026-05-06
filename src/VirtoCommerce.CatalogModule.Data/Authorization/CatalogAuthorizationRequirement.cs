using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Data.Authorization
{
    public sealed class CatalogAuthorizationRequirement(string permission)
        : PermissionAuthorizationRequirement(permission);
}
