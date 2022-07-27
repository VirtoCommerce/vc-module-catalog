using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Web.Search
{
    public class ProductSearchService2 : ProductSearchService
    {
        public ProductSearchService2(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IItemService itemService)
            : base(catalogRepositoryFactory, platformMemoryCache, itemService)
        {
        }

        protected override IQueryable<ItemEntity> BuildQuery(IRepository repository, ProductSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }

        protected override IList<SortInfo> BuildSortExpression(ProductSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
