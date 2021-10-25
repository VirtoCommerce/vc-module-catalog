using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class ProductSearchService2 : ProductSearchService
    {
        public ProductSearchService2(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService) : base(catalogRepositoryFactory, itemService)
        {
        }
        protected override IQueryable<ItemEntity> BuildQuery(ICatalogRepository repository, ProductSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }
        protected override IList<SortInfo> BuildSortExpression(ProductSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
