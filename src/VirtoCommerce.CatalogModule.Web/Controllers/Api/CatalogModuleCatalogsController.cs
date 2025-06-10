using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/catalogs")]
    [Authorize]
    public class CatalogModuleCatalogsController(
        ICatalogService catalogService,
        ICatalogSearchService catalogSearchService,
        IAuthorizationService authorizationService)
        : Controller
    {
        /// <summary>
        /// Get Catalogs list
        /// </summary>
        /// <remarks>Get common and virtual Catalogs list with minimal information included. Returns array of Catalog</remarks>
        [HttpGet]
        [Route("")]
        [Obsolete("use POST api/catalog/catalogs/search instead")]
        public async Task<ActionResult<Catalog[]>> GetCatalogs(string sort = null, int skip = 0, int take = 20)
        {
            var criteria = AbstractTypeFactory<CatalogSearchCriteria>.TryCreateInstance();
            criteria.Sort = sort;
            criteria.Skip = skip;
            criteria.Take = take;

            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await catalogSearchService.SearchNoCloneAsync(criteria);
            return Ok(result.Results);
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<CatalogSearchResult>> SearchCatalogs([FromBody] CatalogSearchCriteria criteria)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, criteria, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var result = await catalogSearchService.SearchNoCloneAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Gets Catalog by id
        /// </summary>
        /// <remarks>Gets Catalog by id with full information loaded</remarks>
        /// <param name="id">The Catalog id.</param>
        /// <param name="responseGroup">Response group</param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Catalog>> GetCatalog([FromRoute] string id, [FromQuery] string responseGroup = null)
        {
            var catalog = await catalogService.GetNoCloneAsync(id, responseGroup);
            if (catalog == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, catalog, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(catalog);
        }

        /// <summary>
        /// Gets catalog by outer id.
        /// </summary>
        /// <remarks>Gets catalog by outer id</remarks>
        /// <param name="outerId">Catalog outer id</param>
        /// <param name="responseGroup">Response group</param>
        [HttpGet]
        [Route("outer/{outerId}")]
        public async Task<ActionResult<Catalog>> GetCatalogByOuterId([FromRoute] string outerId, [FromQuery] string responseGroup = null)
        {
            var catalog = await catalogService.GetByOuterIdNoCloneAsync(outerId, responseGroup);
            if (catalog == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, catalog, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Read));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(catalog);
        }

        /// <summary>
        /// Gets the template for a new catalog
        /// </summary>
        /// <remarks>Gets the template for a new common catalog</remarks>
        [HttpGet]
        [Route("getnew")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public ActionResult<Catalog> GetNewCatalog()
        {
            var retVal = AbstractTypeFactory<Catalog>.TryCreateInstance();
            retVal.Name = "New catalog";
            retVal.SeoInfos = new List<SeoInfo>();
            retVal.Languages = new List<CatalogLanguage>
                {
                    new CatalogLanguage
                    {
                        IsDefault = true,
                        LanguageCode = "en-US"
                    }
                };
            return Ok(retVal);
        }

        /// <summary>
        /// Gets the template for a new virtual catalog
        /// </summary>
        [HttpGet]
        [Route("getnewvirtual")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public ActionResult<Catalog> GetNewVirtualCatalog()
        {
            var retVal = AbstractTypeFactory<Catalog>.TryCreateInstance();
            retVal.Name = "New virtual catalog";
            retVal.IsVirtual = true;
            retVal.Languages = new List<CatalogLanguage>
                {
                    new CatalogLanguage
                    {
                        IsDefault = true,
                        LanguageCode = "en-US"
                    }
                };
            return Ok(retVal);
        }

        /// <summary>
        /// Creates the specified catalog
        /// </summary>
        /// <remarks>Creates the specified catalog</remarks>
        /// <param name="catalog">The catalog to create</param>
        [HttpPost]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Catalog>> CreateCatalog([FromBody] Catalog catalog)
        {
            //Ensure that new category has SeoInfo
            if (catalog.SeoInfos == null || !catalog.SeoInfos.Any())
            {
                var defaultLanguage = catalog.Languages.First(x => x.IsDefault).LanguageCode;
                var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                seoInfo.LanguageCode = defaultLanguage;
                seoInfo.SemanticUrl = "catalog";
                seoInfo.PageTitle = "Catalog";
                catalog.SeoInfos = [seoInfo];
            }

            await catalogService.SaveChangesAsync([catalog]);
            return Ok(catalog);
        }

        /// <summary>
        /// Updates the specified catalog
        /// </summary>
        /// <remarks>Updates the specified catalog.</remarks>
        /// <param name="catalog">The catalog.</param>
        [HttpPut]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpdateCatalog([FromBody] Catalog catalog)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, catalog, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await catalogService.SaveChangesAsync([catalog]);
            return Ok(catalog);
        }

        /// <summary>
        /// Deletes catalog by id
        /// </summary>
        /// <remarks>Deletes catalog by id</remarks>
        /// <param name="id">Catalog id.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteCatalog(string id)
        {
            var catalog = await catalogService.GetNoCloneAsync(id);
            var authorizationResult = await authorizationService.AuthorizeAsync(User, catalog, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Delete));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            await catalogService.DeleteAsync([id]);
            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified Catalog by id
        /// </summary>
        /// <param name="id">Catalog id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchCatalog(string id, [FromBody] JsonPatchDocument<Catalog> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var catalog = await catalogService.GetByIdAsync(id);
            if (catalog == null)
            {
                return NotFound();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, catalog, new CatalogAuthorizationRequirement(ModuleConstants.Security.Permissions.Update));
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(catalog, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await catalogService.SaveChangesAsync([catalog]);

            return NoContent();
        }
    }
}
