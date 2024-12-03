using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule.Core.Search;

public interface IProductConfigurationSearchService : ISearchService<ProductConfigurationSearchCriteria, ProductConfigurationSearchResult, ProductConfiguration>
{
}
