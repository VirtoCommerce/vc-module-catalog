using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Validation
{
    public class CategoryPropertyNameValidator : AbstractValidator<CategoryPropertyValidationRequest>
    {
        private readonly ICategoryService _categoryService;
        private readonly IPropertySearchService _propertySearchService;

        public CategoryPropertyNameValidator(
            ICategoryService categoryService,
            IPropertySearchService propertySearchService)
        {
            _categoryService = categoryService;
            _propertySearchService = propertySearchService;

            AttachValidators();
        }

        private void AttachValidators()
        {
            // 1. Catalog: Prevent adding a property that already exists in the category or is inherited from either parent category or catalog.
            RuleFor(r => r)
              .CustomAsync(async (r, context, _) =>
              {
                  var (isPropertyUniqueForHierarchy, categoryName) = await CheckPropertyUniquenessInCategoryAsync(r);

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


            // 2. Global: Ensure that properties with the same name across the all catalogs have the same type to prevent indexing issues.
            RuleFor(r => r)
               .CustomAsync(async (r, context, _) =>
               {
                   var (isPropertyUniqueForInstance, categoryName) = await CheckPropertyUniquenessAsync(r);

                   if (!isPropertyUniqueForInstance)
                   {
                       context.AddFailure(new ValidationFailure(r.PropertyName, "property-type-name-global-issue")
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

        protected virtual async Task<(bool, string)> CheckPropertyUniquenessInCategoryAsync(CategoryPropertyValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.CategoryId))
            {
                return (true, null);
            }

            var category = await _categoryService.GetNoCloneAsync(request.CategoryId, CategoryResponseGroup.WithProperties.ToString());
            if (category == null)
            {
                return (true, null);
            }

            var propertyNamesForSearch = GetPropertiesNameForSearch(request);

            var property = category.Properties.FirstOrDefault(x => propertyNamesForSearch.Contains(x.Name, StringComparer.OrdinalIgnoreCase));


            if (property != null)
            {
                if (property.IsInherited)
                {
                    return (false, await GetPropertyParentName(property));
                }
                else
                {
                    return (false, category.Name);
                }
            }

            return (true, null);
        }

        private static string[] GetPropertiesNameForSearch(CategoryPropertyValidationRequest request)
        {
            var propertyName = request.PropertyName;
            // "Alcholic % Volume" == "Alcholic_Volume"
            var azureFieldPropertyName = Regex.Replace(propertyName, @"\W", "_");
            return [propertyName, azureFieldPropertyName];
        }

        protected virtual async Task<(bool, string)> CheckPropertyUniquenessAsync(CategoryPropertyValidationRequest request)
        {
            // Allow to create a new property with the same name and same type
            var existingProperty = await FindProperty(request);

            if (existingProperty != null)
            {
                var categoryName = await GetPropertyParentName(existingProperty);
                return (false, categoryName);
            }

            return (true, null);
        }

        private async Task<string> GetPropertyParentName(Property property)
        {
            if (string.IsNullOrEmpty(property.CategoryId))
            {
                // Null means root catalog
                return null;
            }
            else
            {
                return (await _categoryService.GetNoCloneAsync(property.CategoryId, CategoryResponseGroup.Info.ToString()))?.Name;
            }
        }

        private async Task<Property> FindProperty(CategoryPropertyValidationRequest request)
        {
            var propertyNamesForSearch = GetPropertiesNameForSearch(request);

            var properties = await _propertySearchService.SearchPropertiesAsync(new PropertySearchCriteria
            {
                PropertyTypes = [request.PropertyType],
                PropertyNames = propertyNamesForSearch,
                ExcludedPropertyValueTypes = [request.PropertyValueType],
                Take = 1
            });


            return properties.Results.FirstOrDefault();
        }
    }
}
