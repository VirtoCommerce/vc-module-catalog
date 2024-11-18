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
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class ProductConfigurationService : CrudService<ProductConfiguration, ProductConfigurationEntity, ProductConfigurationChangingEvent, ProductConfigurationChangedEvent>, IProductConfigurationService
{
    private readonly Func<ICatalogRepository> _repositoryFactory;

    public ProductConfigurationService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task<ProductConfiguration> GetByProductIdAsync(string productId)
    {
        using var repository = _repositoryFactory();

        // Disable DBContext change tracking for better performance 
        repository.DisableChangesTracking();

        var productEntity = await repository.GetConfigurationByProductIdAsync(productId);

        return productEntity?.ToModel(AbstractTypeFactory<ProductConfiguration>.TryCreateInstance());
    }

    protected override Task<IList<ProductConfigurationEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICatalogRepository)repository).GetConfigurationsByIdsAsync(ids);
    }
}
