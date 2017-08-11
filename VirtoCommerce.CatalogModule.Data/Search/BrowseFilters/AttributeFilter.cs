using System.Xml.Serialization;
using Newtonsoft.Json;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class AttributeFilter : IBrowseFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [JsonIgnore]
        [XmlElement("simple")]
        public AttributeFilterValue[] Values { get; set; }

        [JsonIgnore]
        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }

        [JsonIgnore]
        [XmlElement("display")]
        public FilterDisplayName[] DisplayNames { get; set; }

        [XmlElement("facetSize")]
        public int? FacetSize { get; set; }
    }
}
