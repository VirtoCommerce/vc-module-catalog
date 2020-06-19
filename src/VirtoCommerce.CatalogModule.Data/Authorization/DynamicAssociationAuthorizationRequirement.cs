using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Data.Authorization
{
    public class DynamicAssociationAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public DynamicAssociationAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }
}
