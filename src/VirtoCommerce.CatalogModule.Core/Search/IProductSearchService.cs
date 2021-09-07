using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="SearchService<CatalogProduct>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IProductSearchService
    {
        [Obsolete(@"Need to remove after inherit IProductSearchService from SearchService<CatalogProduct>")]
        Task<ProductSearchResult> SearchProductsAsync(ProductSearchCriteria criteria);
    }
}
