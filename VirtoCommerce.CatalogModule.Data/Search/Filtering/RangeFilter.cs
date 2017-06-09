using System.Collections.Generic;
using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class RangeFilter : ISearchFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlElement("range")]
        public IList<RangeFilterValue> Values { get; set; }

        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }
    }
}
