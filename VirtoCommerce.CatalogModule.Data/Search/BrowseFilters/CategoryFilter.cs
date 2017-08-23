using System;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    [Obsolete]
    public class CategoryFilter : IBrowseFilter
    {
        public string Key { get; set; }

        public int Order { get; set; }

        public CategoryFilterValue[] Values { get; set; }
    }
}
