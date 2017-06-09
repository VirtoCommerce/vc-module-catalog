using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class FilterValueDisplay
    {
        [XmlAttribute("language")]
        public string Language { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("seo")]
        public string Seo { get; set; }
    }
}
