using System;
using System.Collections.Generic;
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

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyDictionaryItemService : CrudService<PropertyDictionaryItem, PropertyDictionaryItemEntity, PropertyDictionaryItemChangingEvent, PropertyDictionaryItemChangedEvent>, IPropertyDictionaryItemService
    {
        public PropertyDictionaryItemService(Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override Task<IList<PropertyDictionaryItemEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((ICatalogRepository)repository).GetPropertyDictionaryItemsByIdsAsync(ids);
        }

        protected override void ConfigureCache(MemoryCacheEntryOptions cacheOptions, string id, PropertyDictionaryItem model)
        {
            base.ConfigureCache(cacheOptions, id, model);

            cacheOptions.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());
        }
    }
}

