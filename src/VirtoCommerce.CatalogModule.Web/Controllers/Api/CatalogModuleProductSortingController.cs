using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Authorize]
[Route("api/catalog/sort-sortings")]
public class CatalogModuleProductSortingController : Controller
{
    private readonly IProductSortingService _productSortingService;
    private readonly IProductSortableFieldService _productSortableFieldService;

    public CatalogModuleProductSortingController(
        IProductSortingService productSortingService,
        IProductSortableFieldService productSortableFieldService)
    {
        _productSortingService = productSortingService;
        _productSortableFieldService = productSortableFieldService;
    }

    /// <summary>
    /// Effective sort sortings for a store (built-in resolvers merged with admin overrides; includes hidden ones).
    /// </summary>
    [HttpGet("store/{storeId}")]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersRead)]
    public async Task<ActionResult<IList<ProductSorting>>> GetOrderings([FromRoute] string storeId)
    {
        var sortings = await _productSortingService.GetSortingsAsync(new ProductSortingContext { StoreId = storeId });
        return Ok(sortings);
    }

    /// <summary>
    /// Fields the admin can pick in a sort clause, derived from the product index schema.
    /// </summary>
    [HttpGet("store/{storeId}/fields")]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersRead)]
    public async Task<ActionResult<IList<ProductSortableField>>> GetSortableFields([FromRoute] string storeId)
    {
        var fields = await _productSortableFieldService.GetSortableFieldsAsync(storeId);
        return Ok(fields);
    }

    [HttpPut("store/{storeId}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersUpdate)]
    public async Task<ActionResult> SaveOrderings([FromRoute] string storeId, [FromBody] ProductSorting[] sortings)
    {
        await _productSortingService.SaveSortingsAsync(storeId, sortings);
        return NoContent();
    }
}
