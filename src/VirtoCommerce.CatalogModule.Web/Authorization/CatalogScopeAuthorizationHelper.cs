using System;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    internal static class CatalogScopeAuthorizationHelper
    {
        public static bool TryGetAuthorizedCatalogIds(string[] requestedCatalogIds, string[] allowedCatalogIds, out string[] authorizedCatalogIds)
        {
            var effectiveAllowedCatalogIds = allowedCatalogIds ?? Array.Empty<string>();

            authorizedCatalogIds = requestedCatalogIds?.Length > 0
                ? requestedCatalogIds.Intersect(effectiveAllowedCatalogIds).ToArray()
                : effectiveAllowedCatalogIds;

            return authorizedCatalogIds.Length > 0;
        }
    }
}
