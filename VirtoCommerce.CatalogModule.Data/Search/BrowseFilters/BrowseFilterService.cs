using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class BrowseFilterService : IBrowseFilterService
    {
        public const string FilteredBrowsingPropertyName = "FilteredBrowsing";

        private readonly IStoreService _storeService;

        public BrowseFilterService(IStoreService storeService)
        {
            _storeService = storeService;
        }

        private static readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof(FilteredBrowsing));
        private static readonly JsonSerializer _jsonSerializer = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
        };

        public virtual IList<IBrowseFilter> GetAllFilters(string storeId)
        {
            var filters = new List<IBrowseFilter>();

            var store = _storeService.GetById(storeId);
            var browsing = GetFilteredBrowsing(store);

            if (browsing != null)
            {
                if (browsing.Attributes != null)
                {
                    filters.AddRange(browsing.Attributes);
                }

                if (browsing.AttributeRanges != null)
                {
                    filters.AddRange(browsing.AttributeRanges);
                }

                if (browsing.Prices != null)
                {
                    filters.AddRange(browsing.Prices);
                }
            }

            return filters.OrderBy(f => f.Order).ToArray();
        }

        public virtual void SetAllFilters(Store store, IList<IBrowseFilter> filters)
        {
            if (store != null)
            {
                var browsing = new FilteredBrowsing
                {
                    Attributes = filters.OfType<AttributeFilter>().ToArray(),
                    AttributeRanges = filters.OfType<RangeFilter>().ToArray(),
                    Prices = filters.OfType<PriceRangeFilter>().ToArray(),
                };

                SetFilteredBrowsing(store, browsing);
            }
        }


        protected virtual FilteredBrowsing GetFilteredBrowsing(Store store)
        {
            FilteredBrowsing result = null;

            var filterSettingValue = store?.GetDynamicPropertyValue(FilteredBrowsingPropertyName, string.Empty);
            if (!string.IsNullOrEmpty(filterSettingValue))
            {
                result = Deserialize(filterSettingValue);
            }

            return result;
        }

        protected virtual void SetFilteredBrowsing(Store store, FilteredBrowsing browsing)
        {
            var value = Serialize(browsing);

            var property = store.DynamicProperties.FirstOrDefault(p => p.Name == FilteredBrowsingPropertyName);
            if (property == null)
            {
                property = new DynamicObjectProperty { Name = FilteredBrowsingPropertyName };
                store.DynamicProperties.Add(property);
            }

            property.Values = new List<DynamicPropertyObjectValue>(new[] { new DynamicPropertyObjectValue { Value = value } });
        }


        // Support JSON for serialization
        private static string Serialize(FilteredBrowsing browsing)
        {
            string result;

            using (var memStream = new MemoryStream())
            {
                browsing.SerializeJson(memStream, _jsonSerializer);
                memStream.Seek(0, SeekOrigin.Begin);

                result = memStream.ReadToString();
            }

            return result;
        }

        // Support both JSON and XML for deserialization
        private static FilteredBrowsing Deserialize(string value)
        {
            FilteredBrowsing result = null;

            if (value != null)
            {
                if (value.StartsWith("<"))
                {
                    // XML
                    var reader = new StringReader(value);
                    result = _xmlSerializer.Deserialize(reader) as FilteredBrowsing;
                }
                else
                {
                    // JSON
                    using (var stringReader = new StringReader(value))
                    using (var jsonTextReader = new JsonTextReader(stringReader))
                    {
                        result = _jsonSerializer.Deserialize<FilteredBrowsing>(jsonTextReader);
                    }
                }
            }

            return result;
        }
    }
}
