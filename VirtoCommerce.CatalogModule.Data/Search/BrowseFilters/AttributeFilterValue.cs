using System;
using System.Xml.Serialization;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    [Serializable]
    public class AttributeFilterValue : IBrowseFilterValue
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}
