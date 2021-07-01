using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class PropertyNameValidator : AbstractValidator<PropertyValidationRequest>
    {
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productSearchService;

        public PropertyNameValidator(IItemService itemService, IProductSearchService productSearchService)
        {
            _itemService = itemService;
            _productSearchService = productSearchService;
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<PropertyValidationRequest> context, CancellationToken cancellation = default)
        {
            var result = new ValidationResult();
            var request = context.InstanceToValidate;

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
                PropertyName = request.Name,
                Take = 1,
                ResponseGroup = ItemResponseGroup.ItemProperties.ToString()
            });

            var duplicateVariation = searchResult.Results.FirstOrDefault();
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
