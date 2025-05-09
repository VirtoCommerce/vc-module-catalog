using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/brand-setting")]
    public class CatalogModuleBrandSettingController : Controller
    {
        private readonly IBrandStoreSettingService _brandSettingService;
        private readonly IBrandStoreSettingSearchService _brandSettingSearchService;

        public CatalogModuleBrandSettingController(
            IBrandStoreSettingService brandSettingService,
            IBrandStoreSettingSearchService brandSettingSearchService)
        {
            _brandSettingService = brandSettingService;
            _brandSettingSearchService = brandSettingSearchService;
        }

        [HttpGet("{id}")]
        //[Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<BrandStoreSetting>> Get([FromRoute] string id)
        {
            var result = await _brandSettingService.GetNoCloneAsync(id);
            return Ok(result);
        }

        [HttpGet("store/{storeId}")]
        //[Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<BrandStoreSetting>> GetByStoreId([FromRoute] string storeId)
        {
            var searchCriteria = AbstractTypeFactory<BrandStoreSettingSearchCriteria>.TryCreateInstance();

            searchCriteria.StoreId = storeId;
            searchCriteria.Take = 1;

            var searchResult = await _brandSettingSearchService.SearchAsync(searchCriteria);
            var result = searchResult.Results?.FirstOrDefault();

            return Ok(result);
        }

        [HttpPost]
        //[Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<BrandStoreSetting>> Create([FromBody] BrandStoreSetting model)
        {
            model.Id = null;
            await _brandSettingService.SaveChangesAsync([model]);
            return Ok(model);
        }

        [HttpPut]
        [Route("")]
        //[Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Update([FromBody] BrandStoreSetting model)
        {
            await _brandSettingService.SaveChangesAsync([model]);
            return NoContent();
        }
    }
}
