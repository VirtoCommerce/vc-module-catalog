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
    public class CatalogSearchService2 : CatalogSearchService
    {
        public CatalogSearchService2(Func<ICatalogRepository> catalogRepositoryFactory, ICatalogService catalogService) : base(catalogRepositoryFactory, catalogService)
        {
        }
        protected override IQueryable<CatalogEntity> BuildQuery(ICatalogRepository repository, CatalogSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }
        protected override IList<SortInfo> BuildSortExpression(CatalogSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
