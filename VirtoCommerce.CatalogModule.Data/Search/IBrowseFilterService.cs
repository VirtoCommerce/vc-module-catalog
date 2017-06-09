using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Data.Search.Filtering;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface IBrowseFilterService
    {
        IList<ISearchFilter> GetFilters(IDictionary<string, object> context);
    }
}
