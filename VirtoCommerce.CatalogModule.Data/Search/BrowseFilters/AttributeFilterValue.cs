using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    [Serializable]
    public class AttributeFilterValue : IBrowseFilterValue
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [Obsolete]
        [JsonIgnore]
        [XmlAttribute("value")]
        public string Value { get; set; }

        [Obsolete]
        [JsonIgnore]
        [XmlAttribute("language")]
        public string Language { get; set; }
    }
}
