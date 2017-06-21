using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class BrowseFilterService : IBrowseFilterService
    {
        private readonly IStoreService _storeService;

        public BrowseFilterService(IStoreService storeService)
        {
            _storeService = storeService;
        }

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(FilteredBrowsing));

        public virtual IList<IBrowseFilter> GetFilters(string storeId)
        {
            var store = _storeService.GetById(storeId);
            return GetStoreFilters(store);
        }

        public virtual IList<IBrowseFilter> GetFilters(IDictionary<string, object> context)
        {
            var store = GetObjectValue(context, "Store") as Store;
            return GetStoreFilters(store);
        }


        protected virtual IList<IBrowseFilter> GetStoreFilters(Store store)
        {
            var filters = new List<IBrowseFilter>();

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

            var filterSettingValue = store.GetDynamicPropertyValue("FilteredBrowsing", string.Empty);

            if (!string.IsNullOrEmpty(filterSettingValue))
            {
                var reader = new StringReader(filterSettingValue);
                result = _serializer.Deserialize(reader) as FilteredBrowsing;
            }

            return result;
        }
    }
}
