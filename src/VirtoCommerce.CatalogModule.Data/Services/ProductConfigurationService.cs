using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class ProductConfigurationService : CrudService<ProductConfiguration, ProductConfigurationEntity, ProductConfigurationChangingEvent, ProductConfigurationChangedEvent>, IProductConfigurationService
{
    public ProductConfigurationService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    // public Task DeleteAsync(IList<string> ids, bool softDelete = false)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task<IList<ProductConfiguration>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task SaveChangesAsync(IList<ProductConfiguration> models)
    // {
    //     throw new NotImplementedException();
    // }

    protected override Task<IList<ProductConfigurationEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICatalogRepository)repository).GetProductConfigurationsByIdsAsync(ids);
    }
}
