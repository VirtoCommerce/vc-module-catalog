using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.AssetsModule.Core.Assets;
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
    private readonly IBlobUrlResolver _blobUrlResolver;

    public ProductConfigurationService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IBlobUrlResolver blobUrlResolver)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _blobUrlResolver = blobUrlResolver;
    }

    protected override Task<IList<ProductConfigurationEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICatalogRepository)repository).GetConfigurationsByIdsAsync(ids, CancellationToken.None);
    }

    protected override IList<ProductConfiguration> ProcessModels(IList<ProductConfigurationEntity> entities, string responseGroup)
    {
        var configurations = base.ProcessModels(entities, responseGroup);

        if (configurations != null && configurations.Count > 0)
        {
            ResolveImageUrls(configurations);
        }

        return configurations;
    }

    private void ResolveImageUrls(IList<ProductConfiguration> configurations)
    {
        var images = configurations.SelectMany(c => c.Sections.SelectMany(s => s.Options.Where(o => o.Product != null && o.Product.Images != null).SelectMany(o => o.Product.Images)));
        images = images.Union(configurations.Where(x => x.Product != null && x.Product.Images != null).SelectMany(x => x.Product.Images));

        foreach (var image in images.Where(x => !string.IsNullOrEmpty(x.Url)))
        {
            image.RelativeUrl = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;
            image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
        }
    }
}
