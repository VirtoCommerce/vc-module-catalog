using System.Threading.Tasks;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface IProductSearchService
    {
        Task<ProductSearchResult> SearchAsync(string storeId, ProductSearch productSearch);
        Task<ProductSearchResult> SearchAsync(SearchCriteria criteria);
    }
}
