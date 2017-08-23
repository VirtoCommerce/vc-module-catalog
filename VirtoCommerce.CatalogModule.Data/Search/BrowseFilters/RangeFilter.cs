using System.Xml.Serialization;
using Newtonsoft.Json;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class RangeFilter : IBrowseFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        public int Order { get; set; }

        [XmlElement("range")]
        public RangeFilterValue[] Values { get; set; }

        [JsonIgnore]
        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }
    }
}
