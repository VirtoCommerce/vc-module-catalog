using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator : AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly ICategoryService _categoryService;
        private readonly IPropertyService _propertyService;
        private readonly Func<int, string, string, CategoryHierarchyIterator> _categoryHierarchyIteratorFactory;

        public CategoryPropertyNameValidator(
            ICategoryService categoryService,
            IPropertyService propertyService,
            Func<int, string, string, CategoryHierarchyIterator> categoryHierarchyIteratorFactory)
        {
            _categoryService = categoryService;
            _propertyService = propertyService;
            _categoryHierarchyIteratorFactory = categoryHierarchyIteratorFactory;

            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(r => r)
                .CustomAsync(async (r, context, _) =>
                {
                    (var isPropertyUniqueForHierarchy, var categoryName) = await CheckPropertyUniquenessAsync(r);

                    if (!isPropertyUniqueForHierarchy)
                    {
                        context.AddFailure(new ValidationFailure(r.PropertyName, "duplicate-property")
                        {
                            CustomState = new
                            {
                                PropertyName = r.PropertyName,
                                CategoryName = categoryName,
                            },
                        });
                    }
                });
        }

        protected virtual async Task<(bool, string)> CheckPropertyUniquenessAsync(CategoryPropertyValidationRequest request)
        {
            var categoryIterator = _categoryHierarchyIteratorFactory(500, request.CatalogId, request.CategoryId);
            Property existingProperty;

            do
            {
                var categoryIds = await categoryIterator.GetNextPageAsync();

                if (categoryIds.IsNullOrEmpty())
                {
                    // Category hierarchy iterator returns only child-relation category ids, excluding the category id itself
                    // so is required to load properties for requested category too
                    categoryIds = categoryIds.Append(request.CategoryId).ToImmutableArray();
                }

                var categories = await _categoryService.GetByIdsAsync(categoryIds.ToArray(), CategoryResponseGroup.WithProperties.ToString());

                var properties = categories.SelectMany(x => x.Properties);

                // If properties are empty and the requested Category missed
                // it might meaning that catalog doesn't contains any categories and 
                if (string.IsNullOrEmpty(request.CategoryId) && !properties.Any())
                {
                    properties = await _propertyService.GetAllCatalogPropertiesAsync(request.CatalogId);
                }

                var requiredPropertyType = EnumUtility.SafeParse(request.PropertyType, PropertyType.Category);

                existingProperty = properties.FirstOrDefault(x => x.Name.EqualsInvariant(request.PropertyName) && x.Type.Equals(requiredPropertyType));

                if (existingProperty != null)
                {
                    break;
                }

            } while (categoryIterator.HasMoreResults);

            var categoryName = string.Empty;

            if (existingProperty != null)
            {
                categoryName = (await _categoryService.GetByIdsAsync(new[] { existingProperty.CategoryId }, CategoryResponseGroup.Info.ToString()))
                    .FirstOrDefault()?.Name;
            }

            return (existingProperty == null, categoryName);
        }
    }
}
