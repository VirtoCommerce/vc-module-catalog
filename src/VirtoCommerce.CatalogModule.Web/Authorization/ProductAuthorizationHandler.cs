using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    public class ProductAuthorizationHandler : AuthorizationHandler<ProductAuthorizationRequirement>
    {
        private readonly IItemService _itemService;

        public ProductAuthorizationHandler(IItemService itemService)
        {
            _itemService = itemService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProductAuthorizationRequirement requirement)
        {
            if (context.User.IsInRole(PlatformConstants.Security.SystemRoles.Administrator))
            {
                context.Succeed(requirement);
                return;
            }

            var productIds = new List<string>();

            if (context.Resource is CatalogProduct product && !product.IsTransient())
            {
                productIds.Add(product.Id);
            }

            if (context.Resource is IEnumerable<CatalogProduct> catalogProducts)
            {
                productIds.AddRange(catalogProducts.Where(x => !x.IsTransient()).Select(x => x.Id));
            }

            var productsToUpdate = await _itemService.GetByIdsAsync(productIds.ToArray(), ItemResponseGroup.ItemInfo.ToString());
            if (!productsToUpdate.Any() || productsToUpdate.All(x => x.CreatedBy == context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }
        }
    }
}
