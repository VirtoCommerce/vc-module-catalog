using System;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    [Obsolete]
    public class CategoryFilterValue : IBrowseFilterValue
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Outline { get; set; }
    }
}
