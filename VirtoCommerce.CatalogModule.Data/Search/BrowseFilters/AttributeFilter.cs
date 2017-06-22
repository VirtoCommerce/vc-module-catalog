using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class AttributeFilter : IBrowseFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlElement("simple")]
        public AttributeFilterValue[] Values { get; set; }

        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }

        [XmlElement("display")]
        public FilterDisplayName[] DisplayNames { get; set; }

        [XmlElement("facetSize")]
        public int? FacetSize { get; set; }
    }
}
