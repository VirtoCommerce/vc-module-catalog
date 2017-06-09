namespace VirtoCommerce.CatalogModule.Data.Search.Filtering
{
    public class CategoryFilter : ISearchFilter
    {
        public string Key { get; set; }

        public CategoryFilterValue[] Values { get; set; }
    }
}
