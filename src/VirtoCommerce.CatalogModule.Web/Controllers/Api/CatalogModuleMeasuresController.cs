using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Options;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using SystemFile = System.IO.File;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/measures")]
    [Authorize]
    public class CatalogModuleMeasuresController : Controller
    {
        private readonly IMeasureService _measureService;
        private readonly IMeasureSearchService _measureSearchService;
        private readonly MeasureOptions _measureOptions;
        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogModuleMeasuresController(
            IMeasureService measureService,
            IMeasureSearchService measureSearchService,
            IOptions<MeasureOptions> measureOptions,
            IHttpClientFactory httpClientFactory)
        {
            _measureService = measureService;
            _measureSearchService = measureSearchService;
            _measureOptions = measureOptions.Value;
            _httpClientFactory = httpClientFactory;
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
        public async Task<ActionResult<IList<Measure>>> CreateMeasure([FromBody] IList<Measure> measures)
        {
            try
            {
                await _measureService.SaveChangesAsync(measures);
            }
            catch (ValidationException ex)
            {
                return BadRequest(GetErrorMessage(ex));
            }

            return Ok(measures);
        }

        [HttpPut]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresUpdate)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateMeasure([FromBody] Measure measure)
        {
            try
            {
                await _measureService.SaveChangesAsync([measure]);
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

        [HttpGet]
        [Route("default")]
        [Authorize(ModuleConstants.Security.Permissions.MeasuresRead)]
        public async Task<ActionResult<IList<Measure>>> GetDefaultMeasures()
        {
            var source = _measureOptions.DefaultSource;

            if (string.IsNullOrEmpty(source))
            {
                throw new InvalidOperationException("Measure source is not defined");
            }

            string json;

            if (source.StartsWith("http"))
            {
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(source);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
            }
            else
            {
                json = await SystemFile.ReadAllTextAsync(source);
            }

            return Ok(JsonConvert.DeserializeObject<List<Measure>>(json));
        }

        private static dynamic GetErrorMessage(ValidationException ex)
        {
            var message = string.Join(Environment.NewLine, ex.Errors.Select(x => x.ErrorMessage));
            return new { message };
        }
    }
}
