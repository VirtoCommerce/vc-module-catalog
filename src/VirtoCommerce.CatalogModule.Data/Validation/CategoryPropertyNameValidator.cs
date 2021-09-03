using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator : AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly SearchService<PropertySearchCriteria, PropertySearchResult, Property, PropertyEntity> _propertySearchService;

        public CategoryPropertyNameValidator(IPropertySearchService propertySearchService)
        {
            _propertySearchService = (SearchService<PropertySearchCriteria, PropertySearchResult, Property, PropertyEntity>)propertySearchService;
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
                var propertySearchResult = await _propertySearchService.SearchAsync(propertySearchCriteria);

                return propertySearchResult.TotalCount == 0;
            }).WithState(r => new
            {
                propertyName = r.PropertyName
            }).WithMessage("duplicate-property").WithName(r => r.PropertyName);
        }
    }
}
