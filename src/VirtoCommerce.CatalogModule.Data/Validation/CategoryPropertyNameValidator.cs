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
            // 1. Global: Ensure that properties with the same name across the all catalogs have the same type to prevent indexing issues.
            RuleFor(r => r)
               .CustomAsync(async (r, context, _) =>
               {
                   var (isPropertyUnique, categoryName) = await CheckPropertyUniquenessInGlobalAsync(r);

                   if (!isPropertyUnique)
                   {
                       context.AddFailure(new ValidationFailure(r.PropertyName, "global-property-name-type")
                       {
                           CustomState = new
                           {
                               PropertyName = r.PropertyName,
                               CategoryName = categoryName,
                           },
                       });
                   }
               });

            // 2. Catalog: Prevent adding a property that already exists in the current category or catalog.
            RuleFor(r => r)
              .CustomAsync(async (r, context, _) =>
              {
                  var (isPropertyUnique, categoryName) = await CheckPropertyUniquenessInParentAsync(r);

                  if (!isPropertyUnique)
                  {
                      context.AddFailure(new ValidationFailure(r.PropertyName, "duplicated-property")
                      {
                          CustomState = new
                          {
                              PropertyName = r.PropertyName,
                              CategoryName = categoryName,
                          },
                      });
                  }
              });

            // 3. Catalog: Prevent adding a property that already inherited from either parent category or catalog.
            RuleFor(r => r)
              .CustomAsync(async (r, context, _) =>
              {
                  var (isPropertyUnique, categoryName) = await CheckPropertyUniquenessInInheritedCategoriesAsync(r);

                  if (!isPropertyUnique)
                  {
                      context.AddFailure(new ValidationFailure(r.PropertyName, "inherited-duplicated-property")
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

        protected virtual async Task<(bool, string)> CheckPropertyUniquenessInParentAsync(CategoryPropertyValidationRequest request)
        {
            var property = await FindPropertyWithSameName(request);

            if (property != null)
            {
                return (false, await GetPropertyParentCategoryName(property));
            }

            return (true, null);
        }

        protected virtual async Task<(bool, string)> CheckPropertyUniquenessInInheritedCategoriesAsync(CategoryPropertyValidationRequest request)
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
                    return (false, await GetPropertyParentCategoryName(property));
                }
                else
                {
                    return (false, category.Name);
                }
            }

            return (true, null);
        }

        protected virtual async Task<(bool, string)> CheckPropertyUniquenessInGlobalAsync(CategoryPropertyValidationRequest request)
        {
            // Allow to create a new property with the same name and same type
            var existingProperty = await FindPropertyWithSameNameAndDifferentType(request);

            if (existingProperty != null)
            {
                var categoryName = await GetPropertyParentCategoryName(existingProperty);
                return (false, categoryName);
            }

            return (true, null);
        }

        private async Task<string> GetPropertyParentCategoryName(Property property)
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

        private async Task<Property> FindPropertyWithSameName(CategoryPropertyValidationRequest request)
        {
            var propertyNamesForSearch = GetPropertiesNameForSearch(request);

            var properties = await _propertySearchService.SearchPropertiesAsync(new PropertySearchCriteria
            {
                PropertyTypes = [request.PropertyType],
                PropertyNames = propertyNamesForSearch,
                CatalogId = request.CatalogId,
                CategoryId = request.CategoryId,
                Take = 1
            });


            return properties.Results.FirstOrDefault();
        }

        private async Task<Property> FindPropertyWithSameNameAndDifferentType(CategoryPropertyValidationRequest request)
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

        private static string[] GetPropertiesNameForSearch(CategoryPropertyValidationRequest request)
        {
            var propertyName = request.PropertyName;
            // "Alcholic % Volume" == "Alcholic_Volume"
            var azureFieldPropertyName = Regex.Replace(propertyName, @"\W", "_");
            return [propertyName, azureFieldPropertyName];
        }


    }
}
