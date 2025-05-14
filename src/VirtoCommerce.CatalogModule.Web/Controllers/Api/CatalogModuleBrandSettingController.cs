using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/brand-setting")]
    public class CatalogModuleBrandSettingController : Controller
    {
        private readonly IBrandStoreSettingService _brandSettingService;

        public CatalogModuleBrandSettingController(IBrandStoreSettingService brandSettingService)
        {
            _brandSettingService = brandSettingService;
        }

        [HttpGet("store/{storeId}")]
        public async Task<ActionResult<BrandStoreSetting>> GetByStoreId([FromRoute] string storeId)
        {
            var result = await _brandSettingService.GetByStoreIdAsync(storeId);

            return Ok(result);
        }

        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Update([FromBody] BrandStoreSetting model)
        {
            await _brandSettingService.UpdateAsync(model);
            return NoContent();
        }
    }
}
