using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class PropertyValidatorService : IPropertyValidatorService
    {
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productSearchService;

        public PropertyValidatorService(IItemService itemService, IProductSearchService productSearchService)
        {
            _itemService = itemService;
            _productSearchService = productSearchService;
        }

        public async Task<ValidationResult> ValidateAsync(PropertyValidationRequest request)
        {
            if (request == null || request.Name.IsNullOrEmpty() || request.ProductId.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = new ValidationResult();

            if (request.Name.EqualsInvariant(request.OriginalName))
            {
                return result;
            }

            var product = await _itemService.GetByIdAsync(request.ProductId, ItemResponseGroup.ItemProperties.ToString());
            if (product == null)
            {
                return result;
            }

            var duplicate = product.Properties.FirstOrDefault(x => x.Name.EqualsInvariant(request.Name) && x.Name != request.OriginalName);

            if (duplicate != null)
            {
                var failure = new ValidationFailure(nameof(Property.Name), "duplicate-property")
                {
                    CustomState = new
                    {
                        propertyName = duplicate.Name
                    }
                };
                return new ValidationResult(new[] { failure });
            }

            var searchResult = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria
            {
                MainProductId = request.ProductId,
                Take = 1000,
                ResponseGroup = ItemResponseGroup.ItemProperties.ToString()
            });

            var duplicateVariation = searchResult.Results.FirstOrDefault(v => v.Properties.Any(x => x.Name.EqualsInvariant(request.Name)));
            if (duplicateVariation != null)
            {
                var failure = new ValidationFailure(nameof(Property.Name), "duplicate-property-in-variation")
                {
                    CustomState = new
                    {
                        propertyName = duplicateVariation.Properties.FirstOrDefault(x => x.Name.EqualsInvariant(request.Name))?.Name,
                        duplicateVariation.Code,
                        duplicateVariation.Name,
                        itemId = duplicateVariation.Id,
                        duplicateVariation.ProductType
                    }
                };

                result = new ValidationResult(new[] { failure });
            }

            return result;
        }
    }
}
