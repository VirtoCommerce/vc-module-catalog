using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface ICategorySearchService
    {
        Task<CategorySearchResult> SearchCategoriesAsync(string storeId, CategorySearch categorySearch);
    }
}
