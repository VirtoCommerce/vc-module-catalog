using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class CatalogModulePropertyDictionaryItemsController : Controller
    {
        private readonly IPropertyDictionaryItemSearchService _propertyDictionarySearchService;
        private readonly IPropertyDictionaryItemService _propertyDictionaryService;
        private readonly IAuthorizationService _authorizationService;

        public CatalogModulePropertyDictionaryItemsController(IPropertyDictionaryItemSearchService propertyDictionarySearchService,
                                                             IPropertyDictionaryItemService propertyDictionaryService,
                                                             IAuthorizationService authorizationService)
        {
            _propertyDictionarySearchService = propertyDictionarySearchService;
            _propertyDictionaryService = propertyDictionaryService;
            _authorizationService = authorizationService;
        }

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
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var result = await _propertyDictionarySearchService.SearchAsync(criteria, clone: true);
            return Ok(result);
        }

        /// <summary>
        /// Creates or updates the specified property dictionary items
        /// </summary>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult> SaveChanges([FromBody] PropertyDictionaryItem[] propertyDictItems)
        {
            await _propertyDictionaryService.SaveChangesAsync(propertyDictItems.ToList());
            return Ok();
        }

        /// <summary>
        /// Delete property dictionary items by ids
        /// </summary>
        /// <param name="ids">The identifiers of objects that needed to be deleted</param>
        [HttpDelete]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public async Task<ActionResult> DeletePropertyDictionaryItems([FromQuery] string[] ids)
        {
            await _propertyDictionaryService.DeleteAsync(ids.ToList());
            return Ok();
        }
    }
}
