using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Route("api/catalog/propertygroups")]
[Authorize]
public class CatalogModulePropertyGroupsController : Controller
{
    private readonly IPropertyGroupService _propertyGroupService;
    private readonly IPropertyGroupSearchService _propertyGroupSearchService;
    private readonly IAuthorizationService _authorizationService;

    public CatalogModulePropertyGroupsController(
        IPropertyGroupService propertyGroupService,
        IPropertyGroupSearchService propertyGroupSearchService,
        IAuthorizationService authorizationService)
    {
        _propertyGroupService = propertyGroupService;
        _propertyGroupSearchService = propertyGroupSearchService;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyGroup>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await _propertyGroupService.GetNoCloneAsync(id, responseGroup);
        if (model == null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, model, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(model);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PropertyGroupSearchResult>> Search([FromBody] PropertyGroupSearchCriteria criteria)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var result = await _propertyGroupSearchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PropertyGroup>> Update([FromBody] PropertyGroup model)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, model, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogMetadataPropertyEdit));
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        await _propertyGroupService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        var propertyGroups = await _propertyGroupService.GetAsync(ids);

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, propertyGroups, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.CatalogDictionaryPropertyEdit));
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        await _propertyGroupService.DeleteAsync(ids);
        return NoContent();
    }
}
