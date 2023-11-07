using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/measures")]
    [Authorize]
    public class CatalogModuleMeasuresController : Controller
    {
        private readonly IMeasureService _measureService;
        private readonly IMeasureSearchService _measureSearchService;

        public CatalogModuleMeasuresController(
            IMeasureService measureService,
            IMeasureSearchService measureSearchService)
        {
            _measureService = measureService;
            _measureSearchService = measureSearchService;
        }


        [HttpGet]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresAccess)]
        public async Task<ActionResult<Measure>> GetContractById(string id)
        {
            var contract = await _measureService.GetNoCloneAsync(id);
            return Ok(contract);
        }

        [HttpPost]
        [Route("search")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresRead)]
        public async Task<ActionResult<MeasureSearchResult>> SearchContracts([FromBody] MeasureSearchCriteria searchCriteria)
        {
            var searchResult = await _measureSearchService.SearchNoCloneAsync(searchCriteria);
            return Ok(searchResult);
        }

        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresCreate)]
        public async Task<ActionResult<Measure>> CreateContract([FromBody] Measure contract)
        {
            try
            {
                await _measureService.SaveChangesAsync(new[] { contract });
            }
            catch (ValidationException ex)
            {
                return BadRequest(GetErrorMessage(ex));
            }

            return Ok(contract);
        }

        [HttpPut]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresUpdate)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateContract([FromBody] Measure contract)
        {
            try
            {
                await _measureService.SaveChangesAsync(new[] { contract });
            }
            catch (ValidationException ex)
            {
                return BadRequest(GetErrorMessage(ex));
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresDelete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteContract([FromQuery] string[] ids)
        {
            await _measureService.DeleteAsync(ids);
            return NoContent();
        }

        private static dynamic GetErrorMessage(ValidationException ex)
        {
            var message = string.Join(Environment.NewLine, ex.Errors.Select(x => x.ErrorMessage));
            return new { message };
        }
    }
}
