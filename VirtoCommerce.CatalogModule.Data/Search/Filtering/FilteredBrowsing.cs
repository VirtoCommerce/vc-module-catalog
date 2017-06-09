using System.Collections.Generic;
using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    [XmlRoot("browsing", Namespace = "", IsNullable = false)]
    public class FilteredBrowsing
    {
        [XmlElement("attribute")]
        public IList<AttributeFilter> Attributes { get; set; }

        [XmlElement("attributeRange")]
        public IList<RangeFilter> AttributeRanges { get; set; }

        [XmlElement("price")]
        public IList<PriceRangeFilter> Prices { get; set; }
    }
}
