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
    }
}
