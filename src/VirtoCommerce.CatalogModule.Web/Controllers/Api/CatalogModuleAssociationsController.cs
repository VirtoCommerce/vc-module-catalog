using System.Net;
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

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products/associations")]
    [Authorize]
    public class CatalogModuleAssociationsController(
        IAssociationService associationService,
        IProductAssociationSearchService productAssociationSearchService
        ) : Controller
    {
        /// <summary>
        /// Returns list of associations for specified product
        /// </summary>
        /// <remarks>Returns list of associations for specified product</remarks>
        /// <param name="productId">Owner product id</param>
        [HttpGet]
        [Route("{productId}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<ProductAssociation[]>> GetProductAssociations(string productId)
        {
            var result = await associationService.GetAssociationsAsync([productId]);
            return Ok(result);
        }

        /// <summary>
        /// Returns list of associations for specified products
        /// </summary>
        /// <remarks>Returns list of associations for specified products</remarks>
        /// <param name="productIds">Array of product ids</param>
        [HttpPost]
        [Route("get/multiple")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<ProductAssociation[]>> GetProductsAssociations([FromBody] string[] productIds)
        {
            var result = await associationService.GetAssociationsAsync(productIds);
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
            await associationService.UpdateAssociationsAsync(associations);
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
        public async Task<ActionResult> Delete([FromQuery] string[] ids)
        {
            await associationService.DeleteAssociationAsync(ids);
            return Ok(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Returns associations by search criteria
        /// </summary>
        /// <remarks>Returns associations by search criteria</remarks>
        [HttpPost]
        [Route("search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<ProductAssociationSearchResult>> Search([FromBody] ProductAssociationSearchCriteria criteria)
        {
            var result = await productAssociationSearchService.SearchProductAssociationsAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Partial update for the specified association by id
        /// </summary>
        /// <param name="id">ProductAssociation id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchAssociation(string id, [FromBody] JsonPatchDocument<ProductAssociation> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var association = await associationService.GetByIdAsync(id);
            if (association == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(association, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await associationService.UpdateAssociationsAsync([association]);

            return NoContent();
        }
    }
}
