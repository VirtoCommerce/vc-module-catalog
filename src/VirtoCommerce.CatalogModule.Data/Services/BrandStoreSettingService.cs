using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class BrandStoreSettingService : CrudService<BrandStoreSetting, BrandStoreSettingEntity, GenericChangedEntryEvent<BrandStoreSetting>, GenericChangedEntryEvent<BrandStoreSetting>>, IBrandStoreSettingService
{
    public BrandStoreSettingService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
    : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    protected override async Task<IList<BrandStoreSettingEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return await ((ICatalogRepository)repository).BrandStoreSettings.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}
