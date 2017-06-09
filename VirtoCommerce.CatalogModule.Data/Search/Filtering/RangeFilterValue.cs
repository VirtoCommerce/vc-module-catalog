using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class RangeFilterValue : ISearchFilterValue
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

        [XmlElement("display")]
        public FilterValueDisplay[] Displays { get; set; }
    }
}
