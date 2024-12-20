using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.ExportImport;

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
            RuleFor(r => r)
               .CustomAsync(async (r, context, _) =>
               {
                   var (isPropertyUniqueForHierarchy, categoryName) = await CheckPropertyUniquenessAsync(r);

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

        /// <summary>
        /// Check the property uniqueness for the whole catalog
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual async Task<(bool, string)> CheckPropertyUniquenessAsync(CategoryPropertyValidationRequest request)
        {
            // Allow to create a new property with the same name and same type
            var existingProperty = await FindProperty(request);

            if (existingProperty != null)
            {
                var categoryName = (await _categoryService.GetNoCloneAsync(existingProperty.CategoryId, CategoryResponseGroup.Info.ToString()))?.Name;
                return (false, categoryName);
            }

            return (true, null);
        }

        private async Task<Property> FindProperty(CategoryPropertyValidationRequest request)
        {
            var propertyName = request.PropertyName;
            // "Alcholic % Volume" == "Alcholic_Volume"
            var azureFieldPropertyName = Regex.Replace(propertyName, @"\W", "_");

            var properties = await _propertySearchService.SearchPropertiesAsync(new PropertySearchCriteria
            {
                PropertyTypes = [request.PropertyType],
                PropertyNames = [propertyName, azureFieldPropertyName],
                ExcludedPropertyValueTypes = [request.PropertyValueType],
                Take = 1
            });


            return properties.Results.FirstOrDefault();
        }
    }
}
