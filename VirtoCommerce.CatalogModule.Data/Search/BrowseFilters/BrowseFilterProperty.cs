using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class BrowseFilterProperty
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public int? Size { get; set; }
        public int ValuesCount => Values?.Count ?? 0;
        public IList<string> Values { get; set; }
    }
}
