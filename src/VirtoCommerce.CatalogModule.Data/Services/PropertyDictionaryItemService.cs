using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyDictionaryItemService : CrudService<PropertyDictionaryItem, PropertyDictionaryItemEntity, PropertyDictionaryItemChangingEvent, PropertyDictionaryItemChangedEvent>, IPropertyDictionaryItemService
    {
        private new readonly Func<ICatalogRepository> _repositoryFactory;
        private new readonly IPlatformMemoryCache _platformMemoryCache;

        public PropertyDictionaryItemService(Func<ICatalogRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher)
          : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<PropertyDictionaryItem[]> GetByIdsAsync(string[] ids)
        {
            var result = await base.GetByIdsAsync(ids);
            return result.ToArray();
        }

        public override Task<IEnumerable<PropertyDictionaryItem>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null)
        {
            var cacheKey = CacheKey.With(GetType(), "GetByIdsAsync", string.Join(",", ids));
            return _platformMemoryCache.GetOrCreateExclusive(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(DictionaryItemsCacheRegion.CreateChangeToken());
                IEnumerable<PropertyDictionaryItem> result;

                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    result = (await repository.GetPropertyDictionaryItemsByIdsAsync(ids.ToArray()))
                        .Select(x => x.ToModel(AbstractTypeFactory<PropertyDictionaryItem>.TryCreateInstance()));
                }
                return result;
            });
        }

        public Task SaveChangesAsync(PropertyDictionaryItem[] dictItems)
        {
            return base.SaveChangesAsync(dictItems);
        }

        public Task DeleteAsync(string[] ids)
        {
            return base.DeleteAsync(ids);
        }

        public override async Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            using (var repository = _repositoryFactory())
            {
                var dbEntities = await repository.GetPropertyDictionaryItemsByIdsAsync(ids.ToArray());

                foreach (var dbEntity in dbEntities)
                {
                    repository.Remove(dbEntity);
                }
                await repository.UnitOfWork.CommitAsync();
            }

            DictionaryItemsCacheRegion.ExpireRegion();
        }

        protected async override Task<IEnumerable<PropertyDictionaryItemEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((ICatalogRepository)repository).GetPropertyDictionaryItemsByIdsAsync(ids.ToArray());
        }
    }
}

