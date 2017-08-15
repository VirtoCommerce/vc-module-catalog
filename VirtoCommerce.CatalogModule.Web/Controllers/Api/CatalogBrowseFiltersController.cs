using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.CatalogModule.Web.Security;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [RoutePrefix("api/catalog/browsefilters")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CatalogBrowseFiltersController : CatalogBaseController
    {
        private readonly IStoreService _storeService;
        private readonly IPropertyService _propertyService;
        private readonly IBrowseFilterService _browseFilterService;

        public CatalogBrowseFiltersController(ISecurityService securityService, IPermissionScopeService permissionScopeService, IStoreService storeService, IPropertyService propertyService, IBrowseFilterService browseFilterService)
            : base(securityService, permissionScopeService)
        {
            _storeService = storeService;
            _propertyService = propertyService;
            _browseFilterService = browseFilterService;
        }

        /// <summary>
        /// Get browse filter properties for store
        /// </summary>
        /// <remarks>
        /// Returns all store catalog properties: selected properties are ordered manually, unselected properties are ordered by name.
        /// </remarks>
        /// <param name="storeId">Store ID</param>
        [HttpGet]
        [Route("properties/{storeId}")]
        [ResponseType(typeof(BrowseFilterProperty[]))]
        public IHttpActionResult GetBrowseFilterProperties(string storeId)
        {
            var store = _storeService.GetById(storeId);
            if (store == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.ReadBrowseFilters, store);

            var allProperties = GetAllCatalogProperties(store.Catalog);
            var selectedPropertyNames = GetSelectedBrowseFilterProperties(store);

            var browseFilterProperties = allProperties
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => ConvertToBrowseFilterProperty(g.FirstOrDefault(), selectedPropertyNames))
                .OrderBy(p => p.Name)
                .ToArray();

            // Keep the selected properties order
            var result = selectedPropertyNames
                .SelectMany(n => browseFilterProperties.Where(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)))
                .Union(browseFilterProperties.Where(p => !selectedPropertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
                .ToArray();

            return Ok(result);
        }

        /// <summary>
        /// Set browse filter properties for store
        /// </summary>
        /// <param name="storeId">Store ID</param>
        /// <param name="browseFilterProperties"></param>
        [HttpPut]
        [Route("properties/{storeId}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult SetBrowseFilterProperties(string storeId, BrowseFilterProperty[] browseFilterProperties)
        {
            var store = _storeService.GetById(storeId);
            if (store == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            CheckCurrentUserHasPermissionForObjects(CatalogPredefinedPermissions.UpdateBrowseFilters, store);

            var allProperties = GetAllCatalogProperties(store.Catalog);

            var selectedPropertyNames = browseFilterProperties
                .Where(p => p.IsSelected)
                .Select(p => p.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            // Keep the selected properties order.
            // There can be many properties with the same name.
            var attributeFilters = selectedPropertyNames
                .SelectMany(n => allProperties.Where(p => string.Equals(p.Name, n, StringComparison.OrdinalIgnoreCase)))
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => ConvertToAttributeFilter(g.Key, g))
                .ToArray();

            _browseFilterService.SetAttributeFilters(store, attributeFilters);
            _storeService.Update(new[] { store });

            return StatusCode(HttpStatusCode.NoContent);
        }


        private string[] GetSelectedBrowseFilterProperties(Store store)
        {
            var result = new List<string>();

            var attributeFilters = _browseFilterService.GetAttributeFilters(store);
            if (attributeFilters != null)
            {
                result.AddRange(attributeFilters.Select(a => a.Key));
            }

            return result.ToArray();
        }

        private IList<Property> GetAllCatalogProperties(string catalogId)
        {
            var properties = _propertyService.GetAllCatalogProperties(catalogId);

            var result = properties
                .GroupBy(p => p.Id)
                .Select(g => g.FirstOrDefault())
                .OrderBy(p => p?.Name)
                .ToArray();

            return result;
        }

        private static BrowseFilterProperty ConvertToBrowseFilterProperty(Property property, IEnumerable<string> selectedPropertyNames)
        {
            return new BrowseFilterProperty
            {
                Name = property.Name,
                IsSelected = selectedPropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase),
            };
        }

        private static AttributeFilter ConvertToAttributeFilter(string propertyName, IEnumerable<Property> properties)
        {
            var values = properties
                .Where(p => p.Dictionary && p.DictionaryValues != null && p.DictionaryValues.Any())
                .SelectMany(p => p.DictionaryValues.Select(v => v.Alias))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .Select(v => new AttributeFilterValue { Id = v })
                .ToArray();

            var result = new AttributeFilter
            {
                Key = propertyName,
                Values = values.Any() ? values : null,
            };

            return result;
        }
    }
}
