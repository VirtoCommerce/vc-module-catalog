using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class RangeFilter : IBrowseFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlElement("range")]
        public RangeFilterValue[] Values { get; set; }

        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }
    }
}
