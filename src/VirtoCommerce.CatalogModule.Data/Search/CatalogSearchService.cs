using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class CatalogSearchService : SearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog, CatalogEntity>,  ICatalogSearchService
    {

        public CatalogSearchService(Func<ICatalogRepositoryForCrud> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            ICatalogService catalogService)
            :base(repositoryFactory, platformMemoryCache, (ICrudService<Catalog>) catalogService)
        {
        }

        public override async Task<CatalogSearchResult> SearchAsync(CatalogSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<CatalogSearchResult>.TryCreateInstance();

            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var sortInfos = BuildSortExpression(criteria);
                var query = BuildQuery(repository, criteria);

                result.TotalCount = await query.CountAsync();
                if (criteria.Take > 0)
                {
                    var ids = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                        .Select(x => x.Id)
                                        .Skip(criteria.Skip).Take(criteria.Take)
                                        .ToArrayAsync();

                    result.Results = (await _crudService.GetByIdsAsync(ids, criteria.ResponseGroup)).OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();
                }
            }
            return result;
        }

        protected override IQueryable<CatalogEntity> BuildQuery(IRepository repository, CatalogSearchCriteria criteria)
        {
            var query = ((ICatalogRepositoryForCrud)repository).Catalogs;
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
                    new SortInfo { SortColumn = nameof(CatalogEntity.Name) }
                };
            }

            return sortInfos;
        }
    }
}
