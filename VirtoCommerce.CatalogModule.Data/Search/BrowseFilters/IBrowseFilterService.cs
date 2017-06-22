using System.Collections.Generic;
using VirtoCommerce.Domain.Store.Model;

namespace VirtoCommerce.CatalogModule.Data.Search.BrowseFilters
{
    public interface IBrowseFilterService
    {
        IList<IBrowseFilter> GetAllFilters(string storeId);
        IList<AttributeFilter> GetAttributeFilters(Store store);
        void SetAttributeFilters(Store store, IList<AttributeFilter> filters);
    }
}
