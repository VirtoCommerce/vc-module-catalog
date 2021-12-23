using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using catalogCore = VirtoCommerce.CatalogModule.Core;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator : AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly ICategoryService _categoryService;
        private readonly IPropertyService _propertyService;
        private readonly ICategoryIndexedSearchService _categoryIndexedSearchService;
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly ISettingsManager _settingsManager;

        public CategoryPropertyNameValidator(
            ICategoryService categoryService,
            IPropertyService propertyService,
            ICategoryIndexedSearchService categoryIndexedSearchService,
            Func<ICatalogRepository> catalogRepositoryFactory,
            ISettingsManager settingsManager
            )
        {
            _categoryService = categoryService;
            _propertyService = propertyService;
            _categoryIndexedSearchService = categoryIndexedSearchService;
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _settingsManager = settingsManager;

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
            var categoryIds = await GetCategoryIds(request);

            var childrenCategories = await _categoryService.GetByIdsAsync(categoryIds.ToArray(), CategoryResponseGroup.WithProperties.ToString());

            var properties = childrenCategories.SelectMany(x => x.Properties);

            var useIndexedSearch = await _settingsManager.GetValueAsync(catalogCore.ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);

            // If useIndexedSearch is off and requested Category is missed, then properties would be empty
            if (string.IsNullOrEmpty(request.CategoryId) && !useIndexedSearch)
            {
                properties = await _propertyService.GetAllCatalogPropertiesAsync(request.CatalogId);
            }

            var requiredPropertyType = EnumUtility.SafeParse(request.PropertyType, PropertyType.Category);

            var existingProperty = properties.FirstOrDefault(x => x.Name.EqualsInvariant(request.PropertyName) && x.Type.Equals(requiredPropertyType));
            var categoryName = string.Empty;

            if (existingProperty != null)
            {
                categoryName = (await _categoryService.GetByIdsAsync(new[] { existingProperty.CategoryId }, CategoryResponseGroup.Info.ToString()))
                    .FirstOrDefault()?.Name;
            }

            return (existingProperty == null, categoryName);
        }

        protected virtual async Task<string[]> GetCategoryIds(CategoryPropertyValidationRequest request)
        {
            var useIndexedSearch = await _settingsManager.GetValueAsync(catalogCore.ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);

            var categoryIds = new List<string>();

            if (useIndexedSearch)
            {
                var indexedSearchCriteria = AbstractTypeFactory<CategoryIndexedSearchCriteria>.TryCreateInstance();
                indexedSearchCriteria.Outline = GetOutline(request);
                indexedSearchCriteria.Take = 10000; // Max elastic search batch

                var searchResult = await _categoryIndexedSearchService.SearchAsync(indexedSearchCriteria);

                categoryIds.AddRange(searchResult.Items.Select(x => x.Id));
            }
            else if (!string.IsNullOrEmpty(request.CategoryId))
            {
                using var catalogRepository = _catalogRepositoryFactory();

                var childrenCategoryIds = await catalogRepository.GetAllChildrenCategoriesIdsAsync(new[] { request.CategoryId });

                categoryIds.AddRange(childrenCategoryIds);
            }

            return categoryIds.ToArray();
        }

        protected virtual string GetOutline(CategoryPropertyValidationRequest request)
        {
            var result = string.IsNullOrEmpty(request.CategoryId)
                ? request.CatalogId
                : $"{request.CatalogId}/{request.CategoryId}";

            return result;
        }
    }
}
