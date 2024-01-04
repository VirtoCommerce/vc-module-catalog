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
                if (context.Resource is IEnumerable<CatalogProduct> products)
                {
                    if (await Compare(products))
                    {
                        context.Succeed(requirement);
                    }
                }
                else if (context.Resource is CatalogProduct product)
                {
                    if (await Compare(new[] { product }))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }

        private async Task<bool> Compare(IEnumerable<CatalogProduct> products)
        {
            var searchCriteria = new ProductSearchCriteria
            {
                ObjectIds = products.Select(x => x.Id).ToArray(),
                SearchInVariations = true,
                ResponseGroup = ItemResponseGroup.WithProperties.ToString()
            };
            var sourceProducts = (await _productSearch.SearchAsync(searchCriteria)).Results;


            foreach (var changedProduct in products)
            {
                var sourceProduct = sourceProducts.FirstOrDefault(x => x.Id == changedProduct.Id);
                if (sourceProduct == null || !CompareProperties(sourceProduct.Properties, changedProduct.Properties))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CompareProperties(IList<Property> source, IList<Property> changed)
        {
            if (source.Count(x => x.Id == null) != changed.Count(x => x.Id == null))
            {
                return false;
            }

            foreach (var sourceProperty in source.Where(x => x.Id == null))
            {
                var changedProperty = changed.FirstOrDefault(x => x.Name == sourceProperty.Name);
                if (changedProperty == null || !ComparePropertyValues(sourceProperty.Values, changedProperty.Values))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ComparePropertyValues(IList<PropertyValue> source, IList<PropertyValue> changed)
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
