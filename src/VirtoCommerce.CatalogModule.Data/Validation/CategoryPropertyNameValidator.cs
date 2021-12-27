using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using catalogCore = VirtoCommerce.CatalogModule.Core;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator : AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly ICategoryService _categoryService;
        private readonly IPropertyService _propertyService;
        private readonly ISettingsManager _settingsManager;
        private readonly Func<int, string, string, CategoryHierarchyIterator> _categoryHierarchyIteratorFactory;

        public CategoryPropertyNameValidator(
            ICategoryService categoryService,
            IPropertyService propertyService,
            ISettingsManager settingsManager,
            Func<int, string, string, CategoryHierarchyIterator> categoryHierarchyIteratorFactory)
        {
            _categoryService = categoryService;
            _propertyService = propertyService;
            _settingsManager = settingsManager;
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

                var childrenCategories = await _categoryService.GetByIdsAsync(categoryIds.ToArray(), CategoryResponseGroup.WithProperties.ToString());

                var properties = childrenCategories.SelectMany(x => x.Properties);

                var useIndexedSearch = await _settingsManager.GetValueAsync(catalogCore.ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);

                // If useIndexedSearch is off and requested Category is missed, then properties would be empty
                if (string.IsNullOrEmpty(request.CategoryId) && !useIndexedSearch)
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
