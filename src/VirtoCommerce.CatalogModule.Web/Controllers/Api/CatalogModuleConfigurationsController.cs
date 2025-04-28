using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api;

[Route("api/catalog/products/configurations")]
[Authorize]
public class CatalogModuleConfigurationsController(
    IProductConfigurationService configurationService,
    IProductConfigurationSearchService configurationSearchService
    ) : Controller
{
    /// <summary>
    /// Get configuration by id.
    /// </summary>
    /// <remarks>Gets configuration by id with full information loaded</remarks>
    /// <param name="id">The configuration id</param>
    [HttpGet]
    [Route("{id}")]
    [Authorize(ModuleConstants.Security.Permissions.ConfigurationsRead)]
    public async Task<ActionResult<ProductConfiguration>> GetConfiguration(string id)
    {
        var configuration = await configurationService.GetNoCloneAsync(id);

        if (configuration == null)
        {
            return NotFound();
        }

        return Ok(configuration);
    }

    /// <summary>
    /// Search configurations
    /// </summary>
    /// <param name="criteria">Search criteria</param>
    [HttpPost]
    [Route("search")]
    [Authorize(ModuleConstants.Security.Permissions.ConfigurationsRead)]
    public async Task<ActionResult<ProductConfigurationSearchResult>> SearchConfigurations([FromBody] ProductConfigurationSearchCriteria criteria)
    {
        var result = await configurationSearchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Create or update the specified configuration.
    /// </summary>
    /// <remarks>If configuration id is null, a new configuration is created. It's updated otherwise</remarks>
    /// <param name="configuration">The configuration.</param>
    [HttpPost]
    [Route("")]
    [Authorize(ModuleConstants.Security.Permissions.ConfigurationsUpdate)]
    public async Task<ActionResult> CreateOrUpdateConfiguration([FromBody] ProductConfiguration configuration)
    {
        // Only the full configuration can be active
        // Sections with Type as ProductConfigurationSectionType.Text and ProductConfigurationSectionType.File can be whithout options
        if ((configuration.Sections is null or []) || configuration.Sections.Where(x => x.Type == ModuleConstants.ConfigurationSectionTypeProduct).Any(x => x.Options is null or []))
        {
            configuration.IsActive = false;
        }

        await configurationService.SaveChangesAsync([configuration]);

        return Ok(configuration);
    }

    /// <summary>
    /// Partial update for the specified Configuration by id
    /// </summary>
    /// <param name="id">Configuration id</param>
    /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
    /// <returns></returns>
    [HttpPatch]
    [Route("{id}")]
    [Authorize(ModuleConstants.Security.Permissions.ConfigurationsUpdate)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> PatchConfiguration(string id, [FromBody] JsonPatchDocument<ProductConfiguration> patchDocument)
    {
        if (patchDocument == null)
        {
            return BadRequest();
        }

        var configuration = await configurationService.GetByIdAsync(id);
        if (configuration == null)
        {
            return NotFound();
        }

        patchDocument.ApplyTo(configuration, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await configurationService.SaveChangesAsync([configuration]);

        return NoContent();
    }
}
