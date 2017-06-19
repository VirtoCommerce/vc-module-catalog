using System;
using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    [Serializable]
    public class AttributeFilterValue : IBrowseFilterValue
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("language")]
        public string Language { get; set; }
    }
}
