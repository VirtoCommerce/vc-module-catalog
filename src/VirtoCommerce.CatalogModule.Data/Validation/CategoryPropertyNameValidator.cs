using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator : AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly IPropertySearchService _propertySearchService;

        public CategoryPropertyNameValidator(IPropertySearchService propertySearchService)
        {
            _propertySearchService = propertySearchService;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(r => r).MustAsync(async (r, _) =>
            {
                var propertySearchCriteria = new PropertySearchCriteria
                {
                    PropertyNames = new[] { r.PropertyName },
                    CategoryId = r.CategoryId
                };
                var propertySearchResult = await _propertySearchService.SearchPropertiesAsync(propertySearchCriteria);

                return propertySearchResult.TotalCount == 0;
            }).WithState(r => new
            {
                propertyName = r.PropertyName
            }).WithMessage("duplicate-property").WithName(r => r.PropertyName);
        }
    }
}
