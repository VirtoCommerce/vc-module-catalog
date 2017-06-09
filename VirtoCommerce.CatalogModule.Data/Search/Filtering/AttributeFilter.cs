using System.Collections.Generic;
using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class AttributeFilter : ISearchFilter
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlElement("simple")]
        public IList<AttributeFilterValue> Values { get; set; }

        [XmlAttribute("localized")]
        public bool IsLocalized { get; set; }

        [XmlElement("display")]
        public IList<FilterDisplayName> DisplayNames { get; set; }

        [XmlElement("facetSize")]
        public int? FacetSize { get; set; }
    }
}
