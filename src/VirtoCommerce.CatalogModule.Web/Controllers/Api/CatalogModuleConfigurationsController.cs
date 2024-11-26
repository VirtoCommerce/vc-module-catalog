using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
public class CatalogModuleConfigurationsController : Controller
{
    private readonly IProductConfigurationService _configurationService;
    private readonly IProductConfigurationSearchService _configurationSearchService;

    public CatalogModuleConfigurationsController(IProductConfigurationService configurationService, IProductConfigurationSearchService configurationSearchService)
    {
        _configurationService = configurationService;
        _configurationSearchService = configurationSearchService;
    }

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
        var configuration = await _configurationService.GetNoCloneAsync(id);

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
        var result = await _configurationSearchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get configuration by product id.
    /// </summary>
    /// <remarks>Gets configuration by product id with full information loaded</remarks>
    /// <param name="productId">The product id</param>
    [HttpGet]
    [Route("~/api/catalog/products/{productId}/configurations")]
    [Authorize(ModuleConstants.Security.Permissions.ConfigurationsRead)]
    public async Task<ActionResult<ProductConfiguration>> GetConfigurationByProductId(string productId)
    {
        var criteria = AbstractTypeFactory<ProductConfigurationSearchCriteria>.TryCreateInstance();
        criteria.ProductId = productId;

        var searchResult = await _configurationSearchService.SearchAsync(criteria);
        var configuration = searchResult.Results.FirstOrDefault();

        if (configuration == null)
        {
            configuration = AbstractTypeFactory<ProductConfiguration>.TryCreateInstance();
            configuration.ProductId = productId;
            configuration.Sections = new List<ProductConfigurationSection>(0);
        }

        return Ok(configuration);
    }

    /// <summary>
    /// Create or update the specified configuration.
    /// </summary>
    /// <remarks>If configuration id is null, a new configuration is created. It's updated otherwise</remarks>
    /// <param name="configuration">The configuration.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the request</param>
    [HttpPost]
    [Route("")]
    [Authorize(ModuleConstants.Security.Permissions.ConfigurationsUpdate)]
    public async Task<ActionResult> CreateOrUpdateConfiguration([FromBody] ProductConfiguration configuration, CancellationToken cancellationToken)
    {
        await _configurationService.SaveChangesAsync(configuration, cancellationToken);

        return Ok(configuration);
    }
}
