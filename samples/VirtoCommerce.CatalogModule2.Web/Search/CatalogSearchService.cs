using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogModule2.Web.Search
{
    public class CatalogSearchService2 : CatalogSearchService
    {
        public CatalogSearchService2(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            ICatalogService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(catalogRepositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<CatalogEntity> BuildQuery(IRepository repository, CatalogSearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }

        protected override IList<SortInfo> BuildSortExpression(CatalogSearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
