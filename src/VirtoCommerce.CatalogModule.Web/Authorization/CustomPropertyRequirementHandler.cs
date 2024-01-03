using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    public sealed class CustomPropertyRequirementHandler : PermissionAuthorizationHandlerBase<CustomPropertyRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        public CustomPropertyRequirementHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        {
            _jsonOptions = jsonOptions.Value;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomPropertyRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                var userPermissions = context.User.FindPermissions(requirement.Permission, _jsonOptions.SerializerSettings);

                if (userPermissions.Count == 0)
                {
                    if (context.Resource is IEnumerable<IHasProperties> haveProperties)
                    {
                        if (haveProperties.All(x => x.Properties.Where(p => p.Id == null).All(p => p.Values.All(v => v.Id != null))))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is IHasProperties hasProperties)
                    {
                        if (hasProperties.Properties.Where(p => p.Id == null).All(x => x.Values.All(v => v.Id != null)))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }
        }
    }
}
