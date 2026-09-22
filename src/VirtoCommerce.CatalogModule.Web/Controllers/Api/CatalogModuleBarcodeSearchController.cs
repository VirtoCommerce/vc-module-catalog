using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Search.Barcodes;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Authorize]
[Route("api/catalog/barcode-search")]
public class CatalogModuleBarcodeSearchController : Controller
{
    private readonly IBarcodeSearchConfigurationService _barcodeSearchConfigurationService;

    public CatalogModuleBarcodeSearchController(IBarcodeSearchConfigurationService barcodeSearchConfigurationService)
    {
        _barcodeSearchConfigurationService = barcodeSearchConfigurationService;
    }

    [HttpGet("store/{storeId}")]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersRead)]
    public async Task<ActionResult<BarcodeSearchSettings>> GetSettings([FromRoute] string storeId)
    {
        var settings = await _barcodeSearchConfigurationService.GetSettingsAsync(storeId);
        return Ok(settings);
    }

    [HttpGet("store/{storeId}/fields")]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersRead)]
    public async Task<ActionResult<IList<BarcodeSearchField>>> GetAvailableFields([FromRoute] string storeId)
    {
        var fields = await _barcodeSearchConfigurationService.GetAvailableFieldsAsync(storeId);
        return Ok(fields);
    }

    [HttpPut("store/{storeId}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersUpdate)]
    public async Task<ActionResult> SaveSettings([FromRoute] string storeId, [FromBody] BarcodeSearchSettings settings)
    {
        if (settings == null)
        {
            return BadRequest(new { message = "Barcode search settings are required." });
        }

        try
        {
            await _barcodeSearchConfigurationService.SaveSettingsAsync(storeId, settings);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = string.Join(Environment.NewLine, ex.Errors.Select(x => x.ErrorMessage)) });
        }
        catch (BarcodeSearchStoreNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }

        return NoContent();
    }
}
