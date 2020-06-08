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
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products/dynamicassociations")]
    public class CatalogModuleDynamicAssociationsController : Controller
    {
        private readonly IDynamicAssociationSearchService _dynamicAssociationSearchService;
        private readonly IDynamicAssociationService _dynamicAssociationService;
        private readonly IAuthorizationService _authorizationService;

        public CatalogModuleDynamicAssociationsController(
            IDynamicAssociationSearchService dynamicAssociationSearchService,
            IDynamicAssociationService dynamicAssociationService,
            IAuthorizationService authorizationService)
        {
            _dynamicAssociationSearchService = dynamicAssociationSearchService;
            _dynamicAssociationService = dynamicAssociationService;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Route("search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<DynamicAssociationSearchResult>> SearchAssociation([FromBody] DynamicAssociationSearchCriteria criteria)
        {
            var result = await _dynamicAssociationSearchService.SearchDynamicAssociationsAsync(criteria);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<DynamicAssociation>> GetAssociationById(string id)
        {
            var result = await _dynamicAssociationService.GetByIdsAsync(new []{id});
            return Ok(result);
        }

        /// <summary>
        /// Create/Update associations.
        /// </summary>
        /// <param name="associations">The dynamic association rules.</param>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<DynamicAssociation[]>> SaveAssociations([FromBody] DynamicAssociation[] associations)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, associations, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            if (!associations.IsNullOrEmpty())
            {
                await _dynamicAssociationService.SaveChangesAsync(associations);
            }
            return Ok(associations);
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
            var associations = (await _dynamicAssociationService.GetByIdsAsync(ids)).FirstOrDefault();
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, associations, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            await _dynamicAssociationService.DeleteAsync(ids);
            return NoContent();
        }
    }
}
