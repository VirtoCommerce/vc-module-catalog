using System;
using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    [Serializable]
    public class AttributeFilterValue : ISearchFilterValue
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("language")]
        public string Language { get; set; }
    }
}
