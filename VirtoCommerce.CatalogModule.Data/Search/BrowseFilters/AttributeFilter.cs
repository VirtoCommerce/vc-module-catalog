using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class AttributeFilter : IBrowseFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlElement("facetSize")]
        public int? FacetSize { get; set; }

        public int Order { get; set; }

        [XmlElement("simple")]
        public AttributeFilterValue[] Values { get; set; }
    }
}
