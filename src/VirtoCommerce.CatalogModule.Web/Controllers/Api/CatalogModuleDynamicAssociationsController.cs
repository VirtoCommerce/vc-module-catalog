using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products/dynamicassociations")]
    [Authorize]
    public class CatalogModuleDynamicAssociationsController : Controller
    {
        private readonly IDynamicAssociationSearchService _searchService;
        private readonly IDynamicAssociationService _service;

        public CatalogModuleDynamicAssociationsController(IDynamicAssociationSearchService searchService, IDynamicAssociationService service)
        {
            _searchService = searchService;
            _service = service;
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<DynamicAssociationSearchResult>> SearchRule([FromBody] DynamicAssociationSearchCriteria criteria)
        {
            var result = await _searchService.SearchDynamicAssociationsAsync(criteria);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<DynamicAssociation>> GetRuleById(string id)
        {
            var result = await _service.GetByIdsAsync(new []{id});
            return Ok(result);
        }

        /// <summary>
        /// Create/Update the specified association.
        /// </summary>
        /// <param name="DynamicAssociation">The dynamic association rule.</param>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<ActionResult<CatalogProduct>> SaveProduct([FromBody] DynamicAssociation association)
        {
            //var authorizationResult = await _authorizationService.AuthorizeAsync(User, association, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            //if (!authorizationResult.Succeeded)
            //{
            //    return Unauthorized();
            //}

            await _service.SaveChangesAsync(new[] { association });
                
            return NoContent();
        }
    }
}
