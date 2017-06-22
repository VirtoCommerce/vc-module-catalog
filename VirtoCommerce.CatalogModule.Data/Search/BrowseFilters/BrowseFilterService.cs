using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
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

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(FilteredBrowsing));

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

            return filters;
        }

        public virtual IList<AttributeFilter> GetAttributeFilters(Store store)
        {
            var browsing = GetFilteredBrowsing(store);
            return browsing?.Attributes;
        }

        public virtual void SetAttributeFilters(Store store, IList<AttributeFilter> filters)
        {
            if (store != null)
            {
                var browsing = GetFilteredBrowsing(store) ?? new FilteredBrowsing();
                browsing.Attributes = filters?.ToArray();
                SetFilteredBrowsing(store, browsing);
            }
        }


        protected virtual object GetObjectValue(IDictionary<string, object> context, string key)
        {
            object result = null;

            if (context.ContainsKey(key))
            {
                result = context[key];
            }

            return result;
        }

        protected virtual FilteredBrowsing GetFilteredBrowsing(Store store)
        {
            FilteredBrowsing result = null;

            var filterSettingValue = store?.GetDynamicPropertyValue(FilteredBrowsingPropertyName, string.Empty);

            if (!string.IsNullOrEmpty(filterSettingValue))
            {
                var reader = new StringReader(filterSettingValue);
                result = _serializer.Deserialize(reader) as FilteredBrowsing;
            }

            return result;
        }

        protected virtual void SetFilteredBrowsing(Store store, FilteredBrowsing browsing)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            _serializer.Serialize(writer, browsing);
            var value = builder.ToString();

            var property = store.DynamicProperties.FirstOrDefault(p => p.Name == FilteredBrowsingPropertyName);
            if (property == null)
            {
                property = new DynamicObjectProperty { Name = FilteredBrowsingPropertyName };
                store.DynamicProperties.Add(property);
            }

            property.Values = new List<DynamicPropertyObjectValue>(new[] { new DynamicPropertyObjectValue { Value = value } });
        }
    }
}
