using System;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class ProductAssociationSearchService2 : ProductAssociationSearchService
    {
        public ProductAssociationSearchService2(Func<ICatalogRepository> catalogRepositoryFactory, IPlatformMemoryCache platformMemoryCache) : base(catalogRepositoryFactory, platformMemoryCache)
        {
        }
    }
}
