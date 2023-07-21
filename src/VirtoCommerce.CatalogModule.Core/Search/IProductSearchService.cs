using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search
{
    public interface IProductSearchService : ISearchService<ProductSearchCriteria, ProductSearchResult, CatalogProduct>
    {
    }
}
