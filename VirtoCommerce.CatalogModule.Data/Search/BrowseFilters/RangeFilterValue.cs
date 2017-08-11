using System.Xml.Serialization;
using Newtonsoft.Json;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class RangeFilterValue : IBrowseFilterValue
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("lower")]
        public string Lower { get; set; }

        [XmlAttribute("upper")]
        public string Upper { get; set; }

        [XmlAttribute("includeLower")]
        public bool IncludeLower { get; set; } = true; // The default value is 'true' for compatibility with previous ranges implementation

        [XmlAttribute("includeUpper")]
        public bool IncludeUpper { get; set; }

        [JsonIgnore]
        [XmlElement("display")]
        public FilterValueDisplay[] Displays { get; set; }
    }
}
