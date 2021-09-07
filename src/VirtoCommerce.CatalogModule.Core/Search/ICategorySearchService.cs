using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="SearchService<Category>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface ICategorySearchService
    {
        [Obsolete(@"Need to remove after inherit ICategorySearchService from SearchService<Category>")]
        Task<CategorySearchResult> SearchCategoriesAsync(CategorySearchCriteria criteria);
    }
}
