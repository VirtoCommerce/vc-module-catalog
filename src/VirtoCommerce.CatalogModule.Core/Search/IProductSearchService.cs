using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface IProductSearchService : ISearchService<ProductSearchCriteria, ProductSearchResult, CatalogProduct>
    {
        [Obsolete("Use SearchAsync()")]
        Task<ProductSearchResult> SearchProductsAsync(ProductSearchCriteria criteria);

        /// <summary>
        /// Returns data from the cache without cloning, which consumes less memory, but returned data must not be modified.
        /// </summary>
        Task<ProductSearchResult> SearchNoCloneAsync(ProductSearchCriteria criteria);
    }
}
