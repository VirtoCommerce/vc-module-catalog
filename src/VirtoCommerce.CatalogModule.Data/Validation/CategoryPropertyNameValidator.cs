using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator: AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly IPropertySearchService _propertySearchService;

        public CategoryPropertyNameValidator(IPropertySearchService propertySearchService)
        {
            _propertySearchService = propertySearchService;
        }
        public override async Task<ValidationResult> ValidateAsync(ValidationContext<CategoryPropertyValidationRequest> context, CancellationToken cancellation = default)
        {
            var result = new ValidationResult();
            var request = context.InstanceToValidate;
            var propertySearchCriteria = new PropertySearchCriteria
            {
                PropertyNames = new []{ request.PropertyName },
                CategoryId = request.CategoryId      
            };
            var propertySearchResult = await _propertySearchService.SearchPropertiesAsync(propertySearchCriteria);
            if (propertySearchResult.TotalCount > 0)
            {
                var failure = new ValidationFailure(request.PropertyName, "duplicate-property")
                {
                    CustomState = new
                    {
                        propertyName = request.PropertyName
                    }
                };
                result = new ValidationResult(new[] { failure });
            }
            return result;
        }
    }
}
