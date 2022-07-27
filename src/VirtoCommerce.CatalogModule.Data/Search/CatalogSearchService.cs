using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CatalogSearchService : SearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog, CatalogEntity>, ICatalogSearchService
    {
        public CatalogSearchService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            ICatalogService catalogService)
            : base(catalogRepositoryFactory, platformMemoryCache, catalogService)
        {
        }

        [Obsolete("Use SearchAsync()")]
        public Task<CatalogSearchResult> SearchCatalogsAsync(CatalogSearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }


        protected override IQueryable<CatalogEntity> BuildQuery(IRepository repository, CatalogSearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).Catalogs;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.Id));
            }
            else if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(CatalogSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(CatalogEntity.Name) },
                };
            }

            return sortInfos;
        }
    }
}
