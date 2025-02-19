using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.BrowseFilters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/aggregationproperties")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    public class CatalogBrowseFiltersController : Controller
    {
        private const string _attributeType = "Attribute";
        private const string _rangeType = "Range";
        private const string _priceRangeType = "PriceRange";

        private readonly IStoreService _storeService;
        private readonly IPropertyService _propertyService;
        private readonly IBrowseFilterService _browseFilterService;
        private readonly IPropertyDictionaryItemSearchService _propDictItemsSearchService;
        private readonly ISettingsManager _settingsManager;

        public CatalogBrowseFiltersController(
            IStoreService storeService,
            IPropertyService propertyService,
            IBrowseFilterService browseFilterService,
            IPropertyDictionaryItemSearchService propDictItemsSearchService,
            ISettingsManager settingsManager)
        {
            _storeService = storeService;
            _propertyService = propertyService;
            _browseFilterService = browseFilterService;
            _propDictItemsSearchService = propDictItemsSearchService;
            _settingsManager = settingsManager;
        }

        /// <summary>
        /// Get aggregation properties for store
        /// </summary>
        /// <remarks>
        /// Returns all store aggregation properties: selected properties are ordered manually, unselected properties are ordered by name.
        /// </remarks>
        /// <param name="storeId">Store ID</param>
        [HttpGet]
        [Route("{storeId}/properties")]
        [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersRead)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<ActionResult<AggregationProperty[]>> GetAggregationProperties(string storeId)
        {
            var store = await _storeService.GetNoCloneAsync(storeId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                return NoContent();
            }

            var allProperties = await GetAllPropertiesAsync(store.Catalog, store.Currencies);
            var selectedProperties = await GetSelectedPropertiesAsync(storeId);

            // Remove duplicates and keep selected properties order
            var result = selectedProperties.Concat(allProperties)
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToArray();

            return Ok(result);
        }

        /// <summary>
        /// Set aggregation properties for store
        /// </summary>
        /// <param name="storeId">Store ID</param>
        /// <param name="browseFilterProperties"></param>
        [HttpPut]
        [Route("{storeId}/properties")]
        [Authorize(ModuleConstants.Security.Permissions.CatalogBrowseFiltersUpdate)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> SetAggregationProperties(string storeId, [FromBody] AggregationProperty[] browseFilterProperties)
        {
            var store = await _storeService.GetNoCloneAsync(storeId, StoreResponseGroup.StoreInfo.ToString());
            if (store == null)
            {
                return NoContent();
            }

            // Add culture-specific aggregations for specific handling multivalue, multilanguage, non-dictionary properties.
            // Look for details at PT-3044, VP-7549

            var catalogProperties = await _propertyService.GetAllCatalogPropertiesAsync(store.Catalog);
            var defaultAggregationSize = await _settingsManager.GetValueAsync<int>(ModuleConstants.Settings.Search.DefaultAggregationSize);

            var browseFilterPropertiesList = new List<AggregationProperty>();
            foreach (var aggregationProperty in browseFilterProperties)
            {
                aggregationProperty.Size ??= defaultAggregationSize;
                browseFilterPropertiesList.Add(aggregationProperty);
                var catalogProperty = catalogProperties.FirstOrDefault(x => x.Name == aggregationProperty.Name);
                // If the property is multilanguage, but not dictionary, let's add synthetic aggregation property for each store culture
                // To allow future facet filtering.
                if (catalogProperty != null &&
                    !catalogProperty.Dictionary &&
                    catalogProperty.Multilanguage)
                {
                    foreach (var lang in store.Languages)
                    {
                        var aggregationPropertyLangSpecific = aggregationProperty.CloneTyped();
                        aggregationPropertyLangSpecific.Name = $"{aggregationPropertyLangSpecific.Name}_{lang.ToLowerInvariant()}";
                        browseFilterPropertiesList.Add(aggregationPropertyLangSpecific);
                    }
                }
            }

            // Filter names must be unique
            // Keep the selected properties order.
            var filters = browseFilterPropertiesList
                .Where(p => p.IsSelected)
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select((g, i) => ConvertToFilter(g.First(), i))
                .Where(f => f != null)
                .ToArray();

            await _browseFilterService.SaveStoreAggregationsAsync(storeId, filters);

            return NoContent();
        }

        [HttpGet]
        [Route("{storeId}/properties/{propertyName}/values")]
        public async Task<ActionResult<string[]>> GetPropertyValues(string storeId, string propertyName)
        {
            var result = Array.Empty<string>();
            var store = await _storeService.GetNoCloneAsync(storeId, StoreResponseGroup.StoreInfo.ToString());
            if (store != null)
            {
                var catalogProperties = await _propertyService.GetAllCatalogPropertiesAsync(store.Catalog);
                var property = catalogProperties.FirstOrDefault(p => p.Name.EqualsInvariant(propertyName) && p.Dictionary);
                if (property != null)
                {
                    var searchResult = await _propDictItemsSearchService.SearchAsync(new PropertyDictionaryItemSearchCriteria { PropertyIds = new[] { property.Id }, Take = int.MaxValue }, clone: true);
                    result = searchResult.Results.Select(x => x.Alias).Distinct().ToArray();
                }
            }
            return Ok(result);
        }


        private async Task<IList<AggregationProperty>> GetAllPropertiesAsync(string catalogId, IEnumerable<string> currencies)
        {
            var result = (await _propertyService.GetAllCatalogPropertiesAsync(catalogId))
                            .Select(p => new AggregationProperty { Type = _attributeType, Name = p.Name })
                            .ToList();

            result.AddRange(currencies.Select(c => new AggregationProperty { Type = _priceRangeType, Name = $"Price {c}", Currency = c }));

            result.Add(new AggregationProperty { Type = _attributeType, Name = "__outline", Size = 0 });
            result.Add(new AggregationProperty { Type = _attributeType, Name = "__outline_named", Size = 0 });
            result.Add(new AggregationProperty { Type = _attributeType, Name = "__path", Size = 0 });

            return result;
        }

        private async Task<IList<AggregationProperty>> GetSelectedPropertiesAsync(string storeId)
        {
            var result = new List<AggregationProperty>();

            var allFilters = await _browseFilterService.GetStoreAggregationsAsync(storeId);
            if (allFilters != null)
            {
                foreach (var filter in allFilters)
                {
                    var property = filter switch
                    {
                        AttributeFilter attributeFilter => new AggregationProperty
                        {
                            IsSelected = true,
                            Type = _attributeType,
                            Name = attributeFilter.Key,
                            Values = attributeFilter.Values?.Select(v => v.Id).OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToArray() ?? [],
                            Size = attributeFilter.FacetSize,
                        },
                        RangeFilter rangeFilter => new AggregationProperty
                        {
                            IsSelected = true,
                            Type = _rangeType,
                            Name = rangeFilter.Key,
                            Values = GetRangeBounds(rangeFilter.Values),
                        },
                        PriceRangeFilter priceRangeFilter => new AggregationProperty
                        {
                            IsSelected = true,
                            Type = _priceRangeType,
                            Name = $"Price {priceRangeFilter.Currency}",
                            Values = GetRangeBounds(priceRangeFilter.Values),
                            Currency = priceRangeFilter.Currency,
                        },
                        _ => null
                    };

                    if (property != null)
                    {
                        result.Add(property);
                    }
                }
            }

            return result;
        }

        private static IList<string> GetRangeBounds(IEnumerable<RangeFilterValue> values)
        {
            return SortStringsAsNumbers(values?.SelectMany(v => new[] { v.Lower, v.Upper }))?.ToArray();
        }

        private static IBrowseFilter ConvertToFilter(AggregationProperty property, int order)
        {
            IBrowseFilter result = null;

            switch (property.Type)
            {
                case _attributeType:
                    result = new AttributeFilter
                    {
                        Order = order,
                        Key = property.Name,
                        FacetSize = property.Size,
                        Values = property.Values?.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).Select(v => new AttributeFilterValue { Id = v }).ToArray(),
                    };
                    break;
                case _rangeType:
                    result = new RangeFilter
                    {
                        Order = order,
                        Key = property.Name,
                        Values = GetRangeFilterValues(property.Values),
                    };
                    break;
                case _priceRangeType:
                    result = new PriceRangeFilter
                    {
                        Order = order,
                        Currency = property.Currency,
                        Values = GetRangeFilterValues(property.Values, isPriceRange: true),
                    };
                    break;
            }

            return result;
        }

        private static RangeFilterValue[] GetRangeFilterValues(IList<string> bounds, bool isPriceRange = false)
        {
            if (bounds.IsNullOrEmpty())
            {
                return null;
            }

            var result = new List<RangeFilterValue>();

            var sortedBounds = SortStringsAsNumbers(bounds).ToList();
            sortedBounds.Add(null);

            string previousBound = null;
            var isFirstRange = true;

            foreach (var bound in sortedBounds)
            {
                // Don't add a range for negative prices if first bound is 0
                if (!isPriceRange || !isFirstRange || bound != "0")
                {
                    var value = new RangeFilterValue
                    {
                        Id = GetRangeId(isFirstRange, bound, previousBound),
                        Lower = previousBound,
                        Upper = bound,
                        // Exclude 0 as lower bound for price range if first bound is 0
                        IncludeLower = !isPriceRange || !isFirstRange || previousBound != "0",
                        IncludeUpper = false,
                    };

                    result.Add(value);
                    isFirstRange = false;
                }

                previousBound = bound;
            }

            return result.Any() ? result.ToArray() : null;
        }

        private static string GetRangeId(bool isFirstRange, string bound, string previousBound)
        {
            if (isFirstRange)
            {
                return $"under-{bound}";
            }

            return bound != null
                ? $"{previousBound}-{bound}"
                : $"over-{previousBound}";
        }

        private static IEnumerable<string> SortStringsAsNumbers(IEnumerable<string> strings)
        {
            return strings
                ?.Where(b => !string.IsNullOrEmpty(b))
                .Select(b => decimal.Parse(b, NumberStyles.Float, CultureInfo.InvariantCulture))
                .OrderBy(b => b)
                .Distinct()
                .Select(b => b.ToString(CultureInfo.InvariantCulture));
        }
    }
}
