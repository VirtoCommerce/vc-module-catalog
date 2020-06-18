using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products/dynamicassociations")]
    [Authorize]
    public class CatalogModuleDynamicAssociationsController : Controller
    {
        private readonly IDynamicAssociationSearchService _dynamicAssociationSearchService;
        private readonly IDynamicAssociationService _dynamicAssociationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDynamicAssociationEvaluator _dynamicAssociationEvaluator;

        public CatalogModuleDynamicAssociationsController(
            IDynamicAssociationSearchService dynamicAssociationSearchService,
            IDynamicAssociationService dynamicAssociationService,
            IAuthorizationService authorizationService,
            IDynamicAssociationEvaluator dynamicAssociationEvaluator)
        {
            _dynamicAssociationSearchService = dynamicAssociationSearchService;
            _dynamicAssociationService = dynamicAssociationService;
            _authorizationService = authorizationService;
            _dynamicAssociationEvaluator = dynamicAssociationEvaluator;
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<DynamicAssociationSearchResult>> SearchAssociation([FromBody] DynamicAssociationSearchCriteria criteria)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                criteria,
                new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var result = await _dynamicAssociationSearchService.SearchDynamicAssociationsAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Gets the dynamic association by Id
        /// </summary>
        /// <param name="id">Association id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<DynamicAssociation>> GetAssociationById(string id)
        {
            var result = (await _dynamicAssociationService.GetByIdsAsync(new[] { id })).FirstOrDefault();

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                result,
                new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            result?.ExpressionTree?.MergeFromPrototype(AbstractTypeFactory<DynamicAssociationRuleTreePrototype>.TryCreateInstance());

            return Ok(result);
        }

        /// <summary>
        /// Get new dynamic association object 
        /// </summary>
        /// <remarks>Return a new dynamic association object with populated dynamic expression tree</remarks>
        [HttpGet]
        [Route("new")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public ActionResult<DynamicAssociation> GetNewDynamicPromotion()
        {
            var result = AbstractTypeFactory<DynamicAssociation>.TryCreateInstance();

            result.ExpressionTree = AbstractTypeFactory<DynamicAssociationRuleTree>.TryCreateInstance();
            result.ExpressionTree.MergeFromPrototype(AbstractTypeFactory<DynamicAssociationRuleTreePrototype>.TryCreateInstance());
            result.IsActive = true;

            return Ok(result);
        }

        /// <summary>
        /// Create/Update associations.
        /// </summary>
        /// <param name="associations">The dynamic association rules.</param>
        [HttpPost]
        [Route("")]
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
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAssociation([FromQuery] string[] ids)
        {
            var dynamicAssociations = await _dynamicAssociationService.GetByIdsAsync(ids);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, dynamicAssociations, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            await _dynamicAssociationService.DeleteAsync(ids);

            return NoContent();
        }

        /// <summary>
        /// Evaluate dynamic associations.
        /// </summary>
        /// <param name="context">Search context</param>
        /// <returns>Associated products ids.</returns>
        [HttpPost]
        [Route("evaluate")]
        public async Task<ActionResult<string[]>> EvaluateDynamicAssociations([FromBody] DynamicAssociationEvaluationContext context)
        {
            ValidateParameters(context);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, context, ModuleConstants.Security.Permissions.Read);

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var result = await _dynamicAssociationEvaluator.EvaluateDynamicAssociationsAsync(context);

            return Ok(result);
        }

        private static void ValidateParameters(DynamicAssociationEvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }
    }
}
