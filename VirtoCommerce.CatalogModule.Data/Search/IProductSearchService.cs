using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface IProductSearchService
    {
        Task<ProductSearchResult> SearchAsync(string storeId, ProductSearch productSearch);
    }
}
