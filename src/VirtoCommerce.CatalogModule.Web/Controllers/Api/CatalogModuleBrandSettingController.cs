using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Authorize]
[Route("api/brand-settings")]
public class CatalogModuleBrandSettingController : Controller
{
    private readonly IBrandSettingService _brandSettingService;

    public CatalogModuleBrandSettingController(IBrandSettingService brandSettingService)
    {
        _brandSettingService = brandSettingService;
    }

    [HttpGet("store/{storeId}")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<BrandStoreSetting>> GetByStoreId([FromRoute] string storeId)
    {
        var result = await _brandSettingService.GetByStoreIdAsync(storeId);

        return Ok(result);
    }

    [HttpPut]
    [Route("")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [Authorize(ModuleConstants.Security.Permissions.Update)]
    public async Task<ActionResult> Update([FromBody] BrandStoreSetting model)
    {
        await _brandSettingService.UpdateAsync(model);
        return NoContent();
    }
}
