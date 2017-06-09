using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class FilterDisplayName
    {
        [XmlAttribute("language")]
        public string Language { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
