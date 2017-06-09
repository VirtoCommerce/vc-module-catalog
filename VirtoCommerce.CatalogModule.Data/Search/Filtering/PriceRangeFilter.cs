using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class PriceRangeFilter : ISearchFilter
    {
        [XmlIgnore]
        public string Key { get; } = "price";

        [XmlElement("range")]
        public RangeFilterValue[] Values { get; set; }

        /// <remarks/>
        [XmlAttribute("currency")]
        public string Currency { get; set; }

        /// <remarks/>
        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }
    }
}
