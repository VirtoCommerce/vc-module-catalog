using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products/associations")]
    [Authorize]
    public class CatalogModuleAssociationsController : Controller
    {
        private readonly IAssociationService _associationService;
        private readonly IProductAssociationSearchService _productAssociationSearchService;

        public CatalogModuleAssociationsController(IAssociationService associationService, IProductAssociationSearchService productAssociationSearchService)
        {
            _associationService = associationService;
            _productAssociationSearchService = productAssociationSearchService;
        }

        /// <summary>
        /// Returns list of associations for specified product
        /// </summary>
        /// <remarks>Returns list of associations for specified product</remarks>
        /// <param name="productId">Owner product id</param>
        [HttpGet]
        [Route("{productId}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<ProductAssociation[]>> GetAllAssociations(string productId)
        {
            var result = await _associationService.GetAssociationsAsync(new string[] { productId });
            return Ok(result);
        }

        /// <summary>
        /// Updates the specified association
        /// </summary>
        /// <remarks>Updates the specified association</remarks>
        /// <param name="associations">The association</param>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpdateAssociations([FromBody] ProductAssociation[] associations)
        {
            await _associationService.UpdateAssociationsAsync(associations);
            return Ok();
        }

        /// <summary>
        /// Deletes specified associations
        /// </summary>
        /// <remarks>Updates the specified association</remarks>
        /// <param name="ids">associations to delete ids</param>
        [HttpDelete]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public async Task<ActionResult> Delete([FromBody]string[] ids)
        {
            await _associationService.DeleteAssociationAsync(ids);
            return Ok(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Returns associations by search criteria
        /// </summary>
        /// <remarks>Returns associations by search criteria</remarks>
        [HttpPost]
        [Route("search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<ProductAssociation[]>> Search([FromBody]ProductAssociationSearchCriteria criteria)
        {
            var result = await _productAssociationSearchService.SearchProductAssociationsAsync(criteria);
            return Ok(result);
        }
    }
}
