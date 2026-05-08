using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class BrowseFilterService(IStoreService storeService) : IBrowseFilterService
    {
        [Obsolete("Kept for migration of legacy values. Use ModuleConstants.Settings.Search.FilteredBrowsing.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public const string FilteredBrowsingPropertyId = "VirtoCommerce.Catalog_FilteredBrowsing_Property";

        [Obsolete("Kept for migration of legacy values. Use ModuleConstants.Settings.Search.FilteredBrowsing.", DiagnosticId = "VC0014", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public const string FilteredBrowsingPropertyName = "FilteredBrowsing";

        private readonly IStoreService _storeService = storeService;
        private static readonly XmlSerializer _xmlSerializer = new(typeof(FilteredBrowsing));
        private static readonly JsonSerializer _jsonSerializer = new()
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
        };

        public async Task<IList<IBrowseFilter>> GetBrowseFiltersAsync(ProductIndexedSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            var aggregations = (await GetAllAggregations(criteria))?.AsQueryable();

            // Check allowed aggregations
            if (criteria.IncludeAggregations != null)
            {
                aggregations = aggregations?.Where(f => criteria.IncludeAggregations.Contains(f.Key, StringComparer.OrdinalIgnoreCase));
            }

            // Check forbidden aggregations
            if (criteria.ExcludeAggregations != null)
            {
                aggregations = aggregations?.Where(f => !criteria.ExcludeAggregations.Contains(f.Key, StringComparer.OrdinalIgnoreCase));
            }

            var result = aggregations
                ?.Where(f => !(f is PriceRangeFilter) || ((PriceRangeFilter)f).Currency.EqualsIgnoreCase(criteria.Currency))
                .ToList();

            return result;
        }

        public virtual async Task<IList<IBrowseFilter>> GetStoreAggregationsAsync(string storeId)
        {
            var serializedValue = await GetSerializedValue(storeId);
            var result = Deserialize(serializedValue);
            return result;
        }

        public virtual async Task SaveStoreAggregationsAsync(string storeId, IList<IBrowseFilter> filters)
        {
            var serializedValue = Serialize(filters);
            await SaveSerializedValue(storeId, serializedValue);
        }


        protected virtual Task<IList<IBrowseFilter>> GetAllAggregations(ProductIndexedSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return GetStoreAggregationsAsync(criteria.StoreId);
        }

        protected virtual async Task<string> GetSerializedValue(string storeId)
        {
            var store = await _storeService.GetNoCloneAsync(storeId);
            if (store == null)
            {
                return null;
            }

            return store.Settings.GetValue<string>(ModuleConstants.Settings.Search.FilteredBrowsing);
        }

        protected virtual async Task SaveSerializedValue(string storeId, string serializedValue)
        {
            var store = await _storeService.GetByIdAsync(storeId);
            if (store == null)
            {
                return;
            }

            store.Settings.First(x => x.Name.EqualsIgnoreCase(Settings.Search.FilteredBrowsing.Name)).Value = serializedValue;

            await _storeService.SaveChangesAsync([store]);
        }


        // Support JSON for serialization
        protected static string Serialize(IList<IBrowseFilter> filters)
        {
            string result = null;

            if (filters != null)
            {
                // Group by type
                var browsing = new FilteredBrowsing
                {
                    Attributes = filters.OfType<AttributeFilter>().ToArray(),
                    AttributeRanges = filters.OfType<RangeFilter>().ToArray(),
                    Prices = filters.OfType<PriceRangeFilter>().ToArray(),
                };

                // Serialize to JSON
                using var memStream = new MemoryStream();
                browsing.SerializeJson(memStream, _jsonSerializer);
                memStream.Seek(0, SeekOrigin.Begin);

                result = memStream.ReadToString();
            }

            return result;
        }

        // Support both JSON and XML for deserialization
        protected static IList<IBrowseFilter> Deserialize(string value)
        {
            IList<IBrowseFilter> result = null;

            if (!string.IsNullOrEmpty(value))
            {
                FilteredBrowsing browsing;

                if (value.StartsWith("<"))
                {
                    // XML
                    var reader = new StringReader(value);
                    browsing = _xmlSerializer.Deserialize(reader) as FilteredBrowsing;
                }
                else
                {
                    // JSON
                    using var stringReader = new StringReader(value);
                    using var jsonTextReader = new JsonTextReader(stringReader);
                    browsing = _jsonSerializer.Deserialize<FilteredBrowsing>(jsonTextReader);
                }

                // Flatten groups
                if (browsing != null)
                {
                    result = new List<IBrowseFilter>();

                    if (browsing.Attributes != null)
                    {
                        result.AddRange(browsing.Attributes);
                    }

                    if (browsing.AttributeRanges != null)
                    {
                        result.AddRange(browsing.AttributeRanges);
                    }

                    if (browsing.Prices != null)
                    {
                        result.AddRange(browsing.Prices);
                    }
                }
            }

            return result?.OrderBy(f => f.Order).ToArray();
        }
    }
}
