using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Authorize]
[Route("api/catalog/sort-orderings")]
public class CatalogModuleSearchOrderingController : Controller
{
    private readonly IProductSearchOrderService _productSearchOrderService;
    private readonly IProductSortableFieldService _productSortableFieldService;

    public CatalogModuleSearchOrderingController(
        IProductSearchOrderService productSearchOrderService,
        IProductSortableFieldService productSortableFieldService)
    {
        _productSearchOrderService = productSearchOrderService;
        _productSortableFieldService = productSortableFieldService;
    }

    /// <summary>
    /// Effective sort orderings for a store (built-in resolvers merged with admin overrides; includes hidden ones).
    /// </summary>
    [HttpGet("store/{storeId}")]
    [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersRead)]
    public async Task<ActionResult<IList<ProductSearchOrdering>>> GetOrderings([FromRoute] string storeId)
    {
        var orderings = await _productSearchOrderService.GetOrderingsAsync(new ProductSearchOrderContext { StoreId = storeId });
        return Ok(orderings);
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
    public async Task<ActionResult> SaveOrderings([FromRoute] string storeId, [FromBody] ProductSearchOrdering[] orderings)
    {
        await _productSearchOrderService.SaveOrderingsAsync(storeId, orderings);
        return NoContent();
    }
}
