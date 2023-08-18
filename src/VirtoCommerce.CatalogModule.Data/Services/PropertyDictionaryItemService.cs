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

        [Obsolete("Use GetAsync(IList<string> ids)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public async Task<PropertyDictionaryItem[]> GetByIdsAsync(string[] ids)
        {
            var result = await GetAsync(ids);
            return result.ToArray();
        }

        [Obsolete("Use SaveChangesAsync(IList<PropertyDictionaryItem> models)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public async Task SaveChangesAsync(PropertyDictionaryItem[] dictItems)
        {
            await base.SaveChangesAsync(dictItems);
        }

        [Obsolete("Use DeleteAsync(IList<string> ids, bool softDelete)", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public async Task DeleteAsync(string[] ids)
        {
            await base.DeleteAsync(ids);
        }

        protected override void ConfigureCache(MemoryCacheEntryOptions cacheOptions, string id, PropertyDictionaryItem model)
        {
            base.ConfigureCache(cacheOptions, id, model);

            cacheOptions.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());
        }
    }
}

