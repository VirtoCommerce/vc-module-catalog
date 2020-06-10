using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class DynamicAssociationService : IDynamicAssociationService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IEventPublisher _eventPublisher;

        public DynamicAssociationService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
        }

        public async Task<DynamicAssociation[]> GetByIdsAsync(string[] itemIds)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", itemIds.OrderBy(x => x)));

            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                var rules = Array.Empty<DynamicAssociation>();

                if (!itemIds.IsNullOrEmpty())
                {
                    using (var catalogRepository = _catalogRepositoryFactory())
                    {
                        //Optimize performance and CPU usage
                        catalogRepository.DisableChangesTracking();

                        var entities = await catalogRepository.DynamicAssociations.Where(x => itemIds.Contains(x.Id)).ToArrayAsync();

                        rules = entities
                            .Select(x => x.ToModel(AbstractTypeFactory<DynamicAssociation>.TryCreateInstance()))
                            .ToArray();

                        cacheEntry.AddExpirationToken(DynamicAssociationCacheRegion.CreateChangeToken(itemIds));
                    }
                }

                return rules;
            });

            return result;
        }

        public async Task SaveChangesAsync(DynamicAssociation[] items)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<DynamicAssociation>>();

            using (var catalogRepository = _catalogRepositoryFactory())
            {
                var ids = items.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray();
                var dbExistProducts = await catalogRepository.GetDynamicAssociationsByIdsAsync(ids);

                foreach (var dynamicAssociation in items)
                {
                    var modifiedEntity = AbstractTypeFactory<DynamicAssociationEntity>.TryCreateInstance().FromModel(dynamicAssociation, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == dynamicAssociation.Id);

                    if (originalEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<DynamicAssociation>(dynamicAssociation, originalEntity.ToModel(AbstractTypeFactory<DynamicAssociation>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        catalogRepository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<DynamicAssociation>(dynamicAssociation, EntryState.Added));
                    }
                }

                await _eventPublisher.Publish(new DynamicAssociationChangingEvent(changedEntries));

                await catalogRepository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                await _eventPublisher.Publish(new DynamicAssociationChangedEvent(changedEntries));
            }

            ClearCache(items);
        }

        public async Task DeleteAsync(string[] itemIds)
        {
            var items = await GetByIdsAsync(itemIds);
            var changedEntries = items
                .Select(x => new GenericChangedEntry<DynamicAssociation>(x, EntryState.Deleted))
                .ToArray();

            using (var catalogRepository = _catalogRepositoryFactory())
            {
                await _eventPublisher.Publish(new DynamicAssociationChangingEvent(changedEntries));

                var dynamicAssociationEntities = await catalogRepository.GetDynamicAssociationsByIdsAsync(itemIds);

                foreach (var dynamicAssociationEntity in dynamicAssociationEntities)
                {
                    catalogRepository.Remove(dynamicAssociationEntity);
                }

                await catalogRepository.UnitOfWork.CommitAsync();

                await _eventPublisher.Publish(new DynamicAssociationChangedEvent(changedEntries));
            }

            ClearCache(items);
        }


        protected virtual void ClearCache(IEnumerable<DynamicAssociation> dynamicAssociations)
        {
            foreach (var dynamicAssociation in dynamicAssociations)
            {
                DynamicAssociationCacheRegion.ExpireEntity(dynamicAssociation);
            }

            DynamicAssociationSearchCacheRegion.ExpireRegion();
        }
    }
}
