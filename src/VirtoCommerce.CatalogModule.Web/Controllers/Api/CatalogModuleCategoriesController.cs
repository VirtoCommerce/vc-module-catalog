using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.BackgroundJobs;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using Permissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/categories")]
    [Authorize]
    public class CatalogModuleCategoriesController(
        ICategoryService categoryService,
        ICatalogService catalogService,
        CatalogEntityAuthorizationService authorizationService)
        : Controller
    {
        /// <summary>
        /// Gets category by id.
        /// </summary>
        /// <param name="id">Category id.</param>
        /// <param name="responseGroup">Response group</param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Category>> GetCategory([FromRoute] string id, [FromQuery] string responseGroup = null)
        {
            var category = await categoryService.GetNoCloneAsync(id, responseGroup);
            if (category == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, category, Permissions.CategoriesRead))
            {
                return Forbid();
            }

            return Ok(category);
        }

        /// <summary>
        /// Gets category by outer id.
        /// </summary>
        /// <remarks>Gets category by outer id (integration key) with full information loaded</remarks>
        /// <param name="outerId">Category outer id</param>
        /// <param name="responseGroup">Response group</param>
        [HttpGet]
        [Route("outer/{outerId}")]
        public async Task<ActionResult<Catalog>> GetCategoryByOuterId([FromRoute] string outerId, [FromQuery] string responseGroup = null)
        {
            var category = await categoryService.GetByOuterIdNoCloneAsync(outerId, responseGroup);
            if (category == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, category, Permissions.CategoriesRead))
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

            if (!await authorizationService.AuthorizeAsync(User, categories, Permissions.CategoriesRead))
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
        /// <remarks>If category ID does not exist in the database, a new category is created. It's updated otherwise</remarks>
        /// <param name="category">The category.</param>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> CreateOrUpdateCategory([FromBody] Category category)
        {
            var categoryExists = await CategoryExistsAsync(category);

            if (!categoryExists)
            {
                //Ensure that new category has SeoInfo
                if (category.SeoInfos == null || !category.SeoInfos.Any())
                {
                    var slugUrl = category.Name.GenerateSlug();
                    if (!slugUrl.IsNullOrEmpty())
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

            var permission = categoryExists
                ? Permissions.CategoriesUpdate
                : Permissions.CategoriesCreate;

            if (!await authorizationService.AuthorizeAsync(User, category, permission))
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

            if (!await authorizationService.AuthorizeAsync(User, categories, Permissions.CategoriesDelete))
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

            if (!await authorizationService.AuthorizeAsync(User, category, Permissions.CategoriesUpdate))
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

        /// <summary>
        /// Create/Update the specified categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        [HttpPost]
        [Route("batch")]
        public async Task<ActionResult<Category[]>> SaveCategories([FromBody] Category[] categories)
        {
            var existingCategoryIds = await GetExistingCategoryIdsAsync(categories);

            var newCategories = categories
                .Where(x => x.Id.IsNullOrEmpty() || !existingCategoryIds.Contains(x.Id))
                .ToArray();

            if (newCategories.Length > 0 &&
                !await authorizationService.AuthorizeAsync(User, newCategories, Permissions.CategoriesCreate))
            {
                return Forbid();
            }

            var existingCategories = categories
                .Where(x => !x.Id.IsNullOrEmpty() && existingCategoryIds.Contains(x.Id))
                .ToArray();

            if (existingCategories.Length > 0 &&
                !await authorizationService.AuthorizeAsync(User, existingCategories, Permissions.CategoriesUpdate))
            {
                return Forbid();
            }

            await categoryService.SaveChangesAsync(categories);

            return Ok(categories);
        }

        [HttpPost("{id}/automatic-links")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public ActionResult UpdateAutomaticLinks(string id)
        {
            BackgroundJob.Enqueue<AutomaticLinksJob>(x => x.UpdateLinks(id, JobCancellationToken.Null));
            return NoContent();
        }

        [HttpDelete("{id}/automatic-links")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public ActionResult DeleteAutomaticLinks(string id)
        {
            BackgroundJob.Enqueue<AutomaticLinksJob>(x => x.DeleteLinks(id, JobCancellationToken.Null));
            return NoContent();
        }

        private async Task<bool> CategoryExistsAsync(Category category)
        {
            return !category.Id.IsNullOrEmpty() &&
                   (await GetExistingCategoryIdsAsync([category])).Contains(category.Id);
        }

        private async Task<HashSet<string>> GetExistingCategoryIdsAsync(IList<Category> categories)
        {
            var ids = categories
                .Where(x => !x.Id.IsNullOrEmpty())
                .Select(x => x.Id)
                .DistinctIgnoreCase()
                .ToArray();

            if (ids.Length == 0)
            {
                return [];
            }

            var existingCategories = await categoryService.GetNoCloneAsync(ids, nameof(CategoryResponseGroup.Info));

            return existingCategories
                .Select(x => x.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
