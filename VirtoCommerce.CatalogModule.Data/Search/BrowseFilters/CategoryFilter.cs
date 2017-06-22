namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public class CategoryFilter : IBrowseFilter
    {
        public string Key { get; set; }

        public CategoryFilterValue[] Values { get; set; }
    }
}
