using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace VirtoCommerce.CatalogModule.Data.Services;

public class PropertyGroupService : CrudService<PropertyGroup, PropertyGroupEntity, PropertyGroupChangingEvent, PropertyGroupChangedEvent>, IPropertyGroupService
{
    public PropertyGroupService(
               Func<ICatalogRepository> repositoryFactory,
               IPlatformMemoryCache platformMemoryCache,
               IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    protected override Task<IList<PropertyGroupEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICatalogRepository)repository).GetPropertyGroupsByIdsAsync(ids, responseGroup);
    }

    protected override void ClearCache(IList<PropertyGroup> models)
    {
        base.ClearCache(models);
        ClearCatalog(models);
    }

    private static void ClearCatalog(IList<PropertyGroup> propertyGroups)
    {
        var catalogIds = propertyGroups.Select(x => x.CatalogId).ToList();

        if (catalogIds.Count != 0)
        {
            CatalogCacheRegion.ExpireRegion();

            foreach (var catalogId in catalogIds.Distinct())
            {
                CatalogTreeCacheRegion.ExpireTokenForKey(catalogId);
            }
        }
    }
}
