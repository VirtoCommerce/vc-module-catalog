using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/categories")]
    [Authorize]
    public class CatalogModuleCategoriesController(
        ICategoryService categoryService,
        ICatalogService catalogService,
        IAuthorizationService authorizationService)
        : Controller
    {
        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <param name="id">Category id.</param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Category>> GetCategory(string id)
        {
            var category = await categoryService.GetNoCloneAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, category, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(category);
        }

        /// <summary>
        /// Gets categories by ids
        /// </summary>
        /// <param name="ids">Categories ids</param>
        ///<param name="respGroup">Response group.</param>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<Category[]>> GetCategoriesByIdsAsync([FromQuery] List<string> ids, [FromQuery] string respGroup = null)
        {
            var categories = await categoryService.GetNoCloneAsync(ids, respGroup);

            var authorizationResult = await authorizationService.AuthorizeAsync(User, categories, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(categories);
        }

        /// <summary>
        /// Get categories by plenty ids
        /// </summary>
        /// <param name="ids">Categories ids</param>
        /// <param name="respGroup">Response group</param>
        /// <returns></returns>
        [HttpPost]
        [Route("plenty")]
        public Task<ActionResult<Category[]>> GetCategoriesByPlentyIds([FromBody] List<string> ids, [FromQuery] string respGroup = null)
        {
            return GetCategoriesByIdsAsync(ids, respGroup);
        }

        /// <summary>
        /// Gets the template for a new category.
        /// </summary>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="parentCategoryId">The parent category id. (Optional)</param>
        [HttpGet]
        [Route("~/api/catalog/{catalogId}/categories/newcategory")]
        public ActionResult<Category> GetNewCategory(string catalogId, [FromQuery] string parentCategoryId = null)
        {
            var category = AbstractTypeFactory<Category>.TryCreateInstance();

            category.ParentId = parentCategoryId;
            category.CatalogId = catalogId;
            category.Code = Guid.NewGuid().ToString().Substring(0, 5);
            category.SeoInfos = new List<SeoInfo>();
            category.IsActive = true;

            return Ok(category);
        }


        /// <summary>
        /// Creates or updates the specified category.
        /// </summary>
        /// <remarks>If category.id is null, a new category is created. It's updated otherwise</remarks>
        /// <param name="category">The category.</param>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> CreateOrUpdateCategory([FromBody] Category category)
        {
            if (category.Id == null)
            {
                //Ensure that new category has SeoInfo
                if (category.SeoInfos == null || !category.SeoInfos.Any())
                {
                    var slugUrl = category.Name.GenerateSlug();
                    if (!string.IsNullOrEmpty(slugUrl))
                    {
                        var catalog = await catalogService.GetNoCloneAsync(category.CatalogId);
                        var defaultLanguage = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
                        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                        seoInfo.LanguageCode = defaultLanguage;
                        seoInfo.SemanticUrl = slugUrl;
                        seoInfo.PageTitle = category.Name.SoftTruncate(ModuleConstants.MaxSEOTitleLength);
                        category.SeoInfos = [seoInfo];
                    }
                }
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, category, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            await categoryService.SaveChangesAsync([category]);
            return Ok(category);

        }

        /// <summary>
        /// Deletes the specified categories by id.
        /// </summary>
        /// <param name="ids">The categories ids.</param>
        [HttpDelete]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteCategory([FromQuery] List<string> ids)
        {
            var categories = await categoryService.GetNoCloneAsync(ids, nameof(CategoryResponseGroup.Info));
            var authorizationResult = await authorizationService.AuthorizeAsync(User, categories, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            await categoryService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified Category by id
        /// </summary>
        /// <param name="id">Category id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchCategory(string id, [FromBody] JsonPatchDocument<Category> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var category = await categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, category, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(category, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await categoryService.SaveChangesAsync([category]);

            return NoContent();
        }
    }
}
