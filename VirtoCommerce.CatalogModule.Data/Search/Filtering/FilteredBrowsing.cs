using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    [XmlRoot("browsing", Namespace = "", IsNullable = false)]
    public class FilteredBrowsing
    {
        [XmlElement("attribute")]
        public AttributeFilter[] Attributes { get; set; }

        [XmlElement("attributeRange")]
        public RangeFilter[] AttributeRanges { get; set; }

        [XmlElement("price")]
        public PriceRangeFilter[] Prices { get; set; }
    }
}
