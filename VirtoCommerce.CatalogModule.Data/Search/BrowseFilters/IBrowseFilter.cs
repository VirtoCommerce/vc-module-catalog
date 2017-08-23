namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public interface IBrowseFilter
    {
        string Key { get; }
        int Order { get; }
    }
}
