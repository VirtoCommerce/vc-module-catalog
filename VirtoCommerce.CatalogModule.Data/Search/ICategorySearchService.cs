using System.Threading.Tasks;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface ICategorySearchService
    {
        Task<CategorySearchResult> SearchAsync(string storeId, CategorySearch categorySearch);
        Task<CategorySearchResult> SearchAsync(SearchCriteria criteria);
    }
}
