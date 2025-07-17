using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class AutomaticLinkQueryService(
    Func<ICatalogRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<AutomaticLinkQuery, AutomaticLinkQueryEntity, AutomaticLinkQueryChangingEvent, AutomaticLinkQueryChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        IAutomaticLinkQueryService
{
    protected override Task<IList<AutomaticLinkQueryEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICatalogRepository)repository).GetAutomaticLinkQueriesByIdsAsync(ids, responseGroup);
    }
}
