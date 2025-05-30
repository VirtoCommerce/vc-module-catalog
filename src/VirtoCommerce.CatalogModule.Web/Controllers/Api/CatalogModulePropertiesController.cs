using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/properties")]
    [Authorize]
    public class CatalogModulePropertiesController(
        IPropertyService propertyService,
        ICategoryService categoryService,
        ICatalogService catalogService,
        IPropertyDictionaryItemSearchService propertyDictionarySearchService,
        IAuthorizationService authorizationService,
        AbstractValidator<PropertyValidationRequest> propertyValidationRequestValidator,
        AbstractValidator<CategoryPropertyValidationRequest> categoryPropertyNameValidator
        ) : Controller
    {
        /// <summary>
        /// Gets all dictionary values that specified property can have.
        /// </summary>
        /// <param name="propertyId">The property id.</param>
        /// <param name="keyword">The keyword. (Optional)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{propertyId}/values")]
        [Obsolete("Use POST api/catalog/properties/dictionaryitems/search instead")]
        public async Task<ActionResult<PropertyDictionaryItem[]>> GetPropertyValues(string propertyId, [FromQuery] string keyword = null)
        {
            var dictValues = await propertyDictionarySearchService.SearchAsync(new PropertyDictionaryItemSearchCriteria { Keyword = keyword, PropertyIds = new[] { propertyId }, Take = int.MaxValue }, clone: true);

            return Ok(dictValues.Results);
        }

        /// <summary>
        /// Gets property metainformation by id.
        /// </summary>
        /// <param name="propertyId">The property id.</param>
        [HttpGet]
        [Route("{propertyId}")]
        public async Task<ActionResult<Property>> GetProperty(string propertyId)
        {
            var property = (await propertyService.GetByIdsAsync([propertyId])).FirstOrDefault();
            if (property == null)
            {
                return NotFound();
            }
            var authorizationResult = await authorizationService.AuthorizeAsync(User, property, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(property);
        }

        /// <summary>
        /// Gets the template for a new catalog property.
        /// </summary>
        /// <param name="catalogId">The catalog id.</param>
        [HttpGet]
        [Route("~/api/catalog/{catalogId}/properties/getnew")]
        public async Task<ActionResult<Property>> GetNewCatalogProperty(string catalogId)
        {
            var catalog = await catalogService.GetNoCloneAsync(catalogId);
            var retVal = new Property
            {
                Id = Guid.NewGuid().ToString(),
                IsNew = true,
                CatalogId = catalog?.Id,
                Name = "new_property",
                Type = PropertyType.Catalog,
                ValueType = PropertyValueType.ShortText,
                Attributes = new List<PropertyAttribute>(),
                DisplayNames = catalog?.Languages.Select(x => new PropertyDisplayName { LanguageCode = x.LanguageCode }).ToList()
            };

            return Ok(retVal);
        }

        /// <summary>
        /// Gets the template for a new category property.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        [HttpGet]
        [Route("~/api/catalog/categories/{categoryId}/properties/getnew")]
        public async Task<ActionResult<Property>> GetNewCategoryProperty(string categoryId)
        {
            var category = await categoryService.GetNoCloneAsync(categoryId, CategoryResponseGroup.Info.ToString());
            var retVal = new Property
            {
                Id = Guid.NewGuid().ToString(),
                IsNew = true,
                CategoryId = categoryId,
                CatalogId = category?.CatalogId,
                Name = "new_property",
                Type = PropertyType.Category,
                ValueType = PropertyValueType.ShortText,
                Attributes = new List<PropertyAttribute>(),
                DisplayNames = category?.Catalog.Languages.Select(x => new PropertyDisplayName { LanguageCode = x.LanguageCode }).ToList()
            };

            return Ok(retVal);
        }

        /// <summary>
        /// Creates or updates the specified property.
        /// </summary>
        /// <remarks>If property.IsNew == True, a new property is created. It's updated otherwise</remarks>
        /// <param name="property">The property.</param>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> SaveProperty([FromBody] Property property)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, property, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogMetadataPropertyEdit));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            await propertyService.SaveChangesAsync([property]);

            return NoContent();
        }

        /// <summary>
        /// Validate name for Product-level (unmanaged) property
        /// </summary>
        [HttpPost]
        [Route("validate-name")]
        public async Task<ActionResult<ValidationResult>> ValidateName([FromBody] PropertyValidationRequest request)
        {
            if (request == null || request.Name.IsNullOrEmpty())
            {
                return BadRequest(request);
            }

            var result = await propertyValidationRequestValidator.ValidateAsync(request);

            return Ok(result);
        }

        [HttpPost]
        [Route("validate-property-name")]
        public async Task<ActionResult<ValidationResult>> ValidatePropertyName([FromBody] CategoryPropertyValidationRequest request)
        {
            if (request == null || request.PropertyName.IsNullOrEmpty() || request.PropertyType.IsNullOrEmpty() || request.CatalogId.IsNullOrEmpty())
            {
                return BadRequest(request);
            }

            var result = await categoryPropertyNameValidator.ValidateAsync(request);

            return Ok(result);
        }

        /// <summary>
        /// Deletes property by id.
        /// </summary>
        /// <param name="id">The property id.</param>
        /// <param name="doDeleteValues">Flag indicating to remove property values from objects as well</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteProperty(string id, bool doDeleteValues = false)
        {
            var property = await propertyService.GetByIdsAsync([id]);

            var authorizationResult = await authorizationService.AuthorizeAsync(User, property, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogMetadataPropertyEdit));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await propertyService.DeleteAsync([id], doDeleteValues);
            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified Property by id
        /// </summary>
        /// <param name="id">Property id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchProperty(string id, [FromBody] JsonPatchDocument<Property> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var property = (await propertyService.GetByIdsAsync([id])).FirstOrDefault();
            if (property == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, property, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogMetadataPropertyEdit));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(property, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await propertyService.SaveChangesAsync([property]);

            return NoContent();
        }
    }
}
