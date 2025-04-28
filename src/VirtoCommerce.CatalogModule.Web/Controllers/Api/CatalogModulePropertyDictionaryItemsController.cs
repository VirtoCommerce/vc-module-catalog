using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/dictionaryitems")]
    [Authorize]
    public class CatalogModulePropertyDictionaryItemsController(
        IPropertyDictionaryItemSearchService propertyDictionarySearchService,
        IPropertyDictionaryItemService propertyDictionaryService,
        IAuthorizationService authorizationService
        ) : Controller
    {
        /// <summary>
        /// Search property dictionary items
        /// </summary>
        /// <param name="criteria">The search criteria</param>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        [Authorize]
        public async Task<ActionResult<PropertyDictionaryItemSearchResult>> SearchPropertyDictionaryItems([FromBody] PropertyDictionaryItemSearchCriteria criteria)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await propertyDictionarySearchService.SearchAsync(criteria, clone: true);
            return Ok(result);
        }

        /// <summary>
        /// Creates or updates the specified property dictionary items
        /// </summary>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.CatalogDictionaryPropertyEdit)]
        public async Task<ActionResult> SaveChanges([FromBody] PropertyDictionaryItem[] propertyDictItems)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, propertyDictItems, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogDictionaryPropertyEdit));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await propertyDictionaryService.SaveChangesAsync(propertyDictItems.ToList());
            return Ok();
        }

        /// <summary>
        /// Delete property dictionary items by ids
        /// </summary>
        /// <param name="ids">The identifiers of objects that needed to be deleted</param>
        [HttpDelete]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.CatalogDictionaryPropertyEdit)]
        public async Task<ActionResult> DeletePropertyDictionaryItems([FromQuery] string[] ids)
        {
            var criteria = new PropertyDictionaryItemSearchCriteria { PropertyIds = ids };
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogDictionaryPropertyEdit));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await propertyDictionaryService.DeleteAsync(ids.ToList());
            return Ok();
        }

        /// <summary>
        /// Partial update for the specified PropertyDictionaryItem by id
        /// </summary>
        /// <param name="id">PropertyDictionaryItem id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchPropertyDictionaryItem(string id, [FromBody] JsonPatchDocument<PropertyDictionaryItem> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var propertyDictItem = (await propertyDictionaryService.GetAsync([id])).FirstOrDefault();
            if (propertyDictItem == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, propertyDictItem, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogDictionaryPropertyEdit));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(propertyDictItem, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await propertyDictionaryService.SaveChangesAsync(new List<PropertyDictionaryItem> { propertyDictItem });

            return NoContent();
        }
    }
}
