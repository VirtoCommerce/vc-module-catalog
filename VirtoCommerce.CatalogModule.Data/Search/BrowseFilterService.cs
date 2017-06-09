using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class BrowseFilterService : IBrowseFilterService
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(FilteredBrowsing));

        public virtual IList<ISearchFilter> GetFilters(IDictionary<string, object> context)
        {
            var filters = new List<ISearchFilter>();

            var store = GetObjectValue(context, "Store") as Store;
            if (store != null)
            {
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
