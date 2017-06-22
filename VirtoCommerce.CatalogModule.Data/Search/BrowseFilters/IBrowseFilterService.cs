using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public interface IBrowseFilterService
    {
        IList<IBrowseFilter> GetFilters(string storeId);
        IList<IBrowseFilter> GetFilters(IDictionary<string, object> context);
    }
}
