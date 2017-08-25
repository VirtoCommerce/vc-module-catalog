using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public interface IBrowseFilterService
    {
        IList<IBrowseFilter> GetAllFilters(string storeId);
        void SaveFilters(string storeId, IList<IBrowseFilter> filters);
    }
}
