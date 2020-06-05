using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class DynamicAssociationSearchService : IDynamicAssociationSearchService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IDynamicAssociationService _dynamicAssociationService;

        public DynamicAssociationSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IPlatformMemoryCache platformMemoryCache, IDynamicAssociationService dynamicAssociationService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _dynamicAssociationService = dynamicAssociationService;
        }

        public async Task<DynamicAssociationSearchResult> SearchDynamicAssociationsAsync(DynamicAssociationSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            var cacheKey = CacheKey.With(GetType(), "SearchDynamicAssociationsAsync", criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(AssociationSearchCacheRegion.CreateChangeToken());
                var result = AbstractTypeFactory<DynamicAssociationSearchResult>.TryCreateInstance();
                using (var repository = _catalogRepositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();
                    var sortInfos = BuildSortExpression(criteria);

                    var query = BuildQuery(repository, criteria);

                    result.TotalCount = await query.CountAsync();


                    if (criteria.Take > 0 && result.TotalCount > 0)
                    {
                        var ids = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                            .Select(x => x.Id)
                            .Skip(criteria.Skip).Take(criteria.Take)
                            .AsNoTracking()
                            .ToArrayAsync();

                        result.Results = (await _dynamicAssociationService.GetByIdsAsync(ids)).OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();
                    }
                }

                return result;
            });
        }

        protected virtual IQueryable<DynamicAssociationEntity> BuildQuery(ICatalogRepository repository, DynamicAssociationSearchCriteria criteria)
        {
            var query = repository.DynamicAssociations;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
            }

            if (!criteria.Groups.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Groups.Contains(x.AssociationType));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(DynamicAssociationSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[] {new SortInfo {SortColumn = nameof(DynamicAssociation.Name)}};
            }

            return sortInfos;
        }
    }
}
