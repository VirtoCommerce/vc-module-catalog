using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Route("api/catalog/propertygroups")]
[Authorize]
public class CatalogModulePropertyGroupsController : Controller
{
    private readonly IPropertyGroupService _propertyGroupService;
    private readonly IAuthorizationService _authorizationService;

    public CatalogModulePropertyGroupsController(IPropertyGroupService propertyGroupService, IAuthorizationService authorizationService)
    {
        _propertyGroupService = propertyGroupService;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public Task<ActionResult<PropertyGroup>> Create([FromBody] PropertyGroup model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    public async Task<ActionResult<PropertyGroup>> Update([FromBody] PropertyGroup model)
    {
        await _propertyGroupService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyGroup>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await _propertyGroupService.GetNoCloneAsync(id, responseGroup);
        return Ok(model);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await _propertyGroupService.DeleteAsync(ids);
        return NoContent();
    }
}
