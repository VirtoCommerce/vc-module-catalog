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
        public async Task<ActionResult<Measure>> GetMeasureById(string id)
        {
            var measure = await _measureService.GetNoCloneAsync(id);
            return Ok(measure);
        }

        [HttpPost]
        [Route("search")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresRead)]
        public async Task<ActionResult<MeasureSearchResult>> SearchMeasures([FromBody] MeasureSearchCriteria searchCriteria)
        {
            var searchResult = await _measureSearchService.SearchNoCloneAsync(searchCriteria);
            return Ok(searchResult);
        }

        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresCreate)]
        public async Task<ActionResult<Measure>> CreateMeasure([FromBody] Measure measure)
        {
            try
            {
                await _measureService.SaveChangesAsync(new[] { measure });
            }
            catch (ValidationException ex)
            {
                return BadRequest(GetErrorMessage(ex));
            }

            return Ok(measure);
        }

        [HttpPut]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresUpdate)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateMeasure([FromBody] Measure measure)
        {
            try
            {
                await _measureService.SaveChangesAsync(new[] { measure });
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
        public async Task<ActionResult> DeleteMeasures([FromQuery] string[] ids)
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
