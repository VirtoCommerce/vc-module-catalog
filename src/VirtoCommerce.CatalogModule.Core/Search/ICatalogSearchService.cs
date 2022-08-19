using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface ICatalogSearchService : ISearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog>
    {
        [Obsolete("Use SearchAsync()")]
        Task<CatalogSearchResult> SearchCatalogsAsync(CatalogSearchCriteria criteria);
    }
}
