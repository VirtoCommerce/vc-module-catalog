using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface ICatalogSearchService
    {
        [Obsolete(@"Need to remove after inheriting ICatalogSearchService from SearchService.")]
        Task<CatalogSearchResult> SearchCatalogsAsync(CatalogSearchCriteria criteria);
    }
}
