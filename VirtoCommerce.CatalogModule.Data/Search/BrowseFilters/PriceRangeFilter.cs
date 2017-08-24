using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class PriceRangeFilter : IBrowseFilter
    {
        [XmlIgnore]
        public string Key { get; } = "price";

        [XmlAttribute("currency")]
        public string Currency { get; set; }

        public int Order { get; set; }

        [XmlElement("range")]
        public RangeFilterValue[] Values { get; set; }
    }
}
