using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CatalogModule.Web.Authorization
{
    public sealed class CustomPropertyRequirementHandler : PermissionAuthorizationHandlerBase<CustomPropertyRequirement>
    {
        private readonly IProductSearchService _productSearch;
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        public CustomPropertyRequirementHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions, IProductSearchService productSearch)
        {
            _productSearch = productSearch;
            _jsonOptions = jsonOptions.Value;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomPropertyRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                var userPermissions = context.User.FindPermissions(requirement.Permission, _jsonOptions.SerializerSettings);

                if (userPermissions.Count != 0)
                {
                    return;
                }
                switch (context.Resource)
                {
                    case IEnumerable<CatalogProduct> products when await CustomPropertyChanged(products):
                    case CatalogProduct product when await CustomPropertyChanged(new[] { product }):
                        context.Succeed(requirement);
                        break;
                }
            }
        }

        private async Task<bool> CustomPropertyChanged(IEnumerable<CatalogProduct> products)
        {
            var searchCriteria = new ProductSearchCriteria
            {
                ObjectIds = products.Select(x => x.Id).ToArray(),
                SearchInVariations = true,
                ResponseGroup = ItemResponseGroup.WithProperties.ToString(),
                Take = products.Count()
            };
            var sourceProducts = (await _productSearch.SearchAsync(searchCriteria)).Results;

            foreach (var changedProduct in products)
            {
                var sourceProduct = sourceProducts.FirstOrDefault(x => x.Id == changedProduct.Id);
                if (sourceProduct == null || !CustomPropertiesChanged(sourceProduct.Properties, changedProduct.Properties))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CustomPropertiesChanged(IList<Property> source, IList<Property> changed)
        {
            if (source.Count(x => x.Id == null) != changed.Count(x => x.Id == null))
            {
                return false;
            }

            foreach (var sourceProperty in source.Where(x => x.Id == null))
            {
                var changedProperty = changed.FirstOrDefault(x => x.Name == sourceProperty.Name);
                if (changedProperty == null || !CustomPropertyValuesChanged(sourceProperty.Values, changedProperty.Values))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CustomPropertyValuesChanged(IList<PropertyValue> source, IList<PropertyValue> changed)
        {
            if (source.Count != changed.Count)
            {
                return false;
            }

            foreach (var sourceValue in source)
            {
                var changedValue = changed.FirstOrDefault(x => x.Id == sourceValue.Id);
                if (changedValue == null || changedValue.PropertyName != sourceValue.PropertyName)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
