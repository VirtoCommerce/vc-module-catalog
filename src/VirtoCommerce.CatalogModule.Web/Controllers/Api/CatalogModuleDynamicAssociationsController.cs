using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products/dynamicassociations")]
    [Authorize]
    public class CatalogModuleDynamicAssociationsController : Controller
    {
        private readonly IDynamicAssociationSearchService _searchService;
        private readonly IDynamicAssociationService _service;
        private readonly IAuthorizationService _authorizationService;

        public CatalogModuleDynamicAssociationsController(IDynamicAssociationSearchService searchService, IDynamicAssociationService service, IAuthorizationService authorizationService)
        {
            _searchService = searchService;
            _service = service;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<DynamicAssociationSearchResult>> SearchAssociation([FromBody] DynamicAssociationSearchCriteria criteria)
        {
            var result = await _searchService.SearchDynamicAssociationsAsync(criteria);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<DynamicAssociation>> GetAssociationById(string id)
        {
            var result = await _service.GetByIdsAsync(new []{id});
            return Ok(result);
        }

        /// <summary>
        /// Create/Update the specified association.
        /// </summary>
        /// <param name="association">The dynamic association rule.</param>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<ActionResult<CatalogProduct>> SaveAssociations([FromBody] DynamicAssociation association)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, association, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            await _service.SaveChangesAsync(new[] { association });
                
            return NoContent();
        }

        /// <summary>
        /// Deletes association rule by id.
        /// </summary>
        /// <remarks>Deletes association rule by id</remarks>
        /// <param name="ids">association ids.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAssociation([FromQuery]string[] ids)
        {
            var associations = (await _service.GetByIdsAsync(ids)).FirstOrDefault();
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, associations, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            await _service.DeleteAsync(ids);
            return NoContent();
        }
    }
}
