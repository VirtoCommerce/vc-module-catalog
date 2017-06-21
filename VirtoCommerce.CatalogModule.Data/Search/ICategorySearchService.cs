using System.Threading.Tasks;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public interface ICategorySearchService
    {
        Task<CategorySearchResult> SearchAsync(CategorySearchCriteria criteria);
    }
}
