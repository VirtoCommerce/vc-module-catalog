using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface ICategorySearchService
    {
        Task<CategorySearchResult> SearchAsync(string storeId, CategorySearch categorySearch);
    }
}
