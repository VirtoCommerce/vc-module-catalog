using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Data.Authorization
{
    public class CustomPropertyRequirement : PermissionAuthorizationRequirement
    {
        public CustomPropertyRequirement() : base(ModuleConstants.Security.Permissions.CatalogCustomPropertyEdit)
        {
        }
    }
}
