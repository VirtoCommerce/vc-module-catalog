using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
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
        return ((ICatalogRepository)repository).GetConfigurationsByIdsAsync(
            ids,
            responseGroup ?? ProductConfigurationResponseGroup.Full.ToString(),
            CancellationToken.None);
    }

    // Override the existence-check load used by SaveChangesAsync only. We narrow to the
    // (Sections + Options) graph and intentionally exclude `WithProducts`: loading the
    // option-referenced Items into the tracker brings the `Item ↔ ProductConfiguration`
    // 1:1 cascade-delete relationship (CatalogDbContext.cs:410) under EF fixup, which is
    // unnecessary for save and risks tracker-state mutations on commit.
    //
    // Reads still get the full graph (LoadEntities passes the caller's responseGroup
    // through, defaulting to Full).
    protected override Task<IList<ProductConfigurationEntity>> LoadExistingEntities(IRepository repository, IList<ProductConfiguration> models)
    {
        var ids = models?.Where(x => !x.IsTransient()).Select(x => x.Id).ToList() ?? [];
        if (ids.Count == 0)
        {
            return Task.FromResult<IList<ProductConfigurationEntity>>(Array.Empty<ProductConfigurationEntity>());
        }

        var saveResponseGroup = (ProductConfigurationResponseGroup.Sections | ProductConfigurationResponseGroup.Options).ToString();
        return LoadEntities(repository, ids, saveResponseGroup);
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

    protected override Task BeforeSaveChanges(IList<ProductConfiguration> models)
    {
        EnsureSingleDefaultOptionPerSection(models);
        return base.BeforeSaveChanges(models);
    }

    private void ResolveImageUrls(IList<ProductConfiguration> configurations)
    {
        // Walk configurations once, collecting images from (a) each option's referenced product and
        // (b) the configuration's own main product. Materializing to a HashSet de-dupes shared image
        // instances (an Image can be referenced by both an option and the main product if they
        // point to the same Item).
        var images = new HashSet<Image>();
        foreach (var configuration in configurations)
        {
            if (configuration.Product?.Images != null)
            {
                foreach (var image in configuration.Product.Images)
                {
                    images.Add(image);
                }
            }

            if (configuration.Sections == null)
            {
                continue;
            }

            foreach (var section in configuration.Sections)
            {
                if (section.Options == null)
                {
                    continue;
                }

                foreach (var option in section.Options)
                {
                    if (option.Product?.Images == null)
                    {
                        continue;
                    }

                    foreach (var image in option.Product.Images)
                    {
                        images.Add(image);
                    }
                }
            }
        }

        foreach (var image in images.Where(i => !string.IsNullOrEmpty(i.Url)))
        {
            image.RelativeUrl = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;
            image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
        }
    }

    private static void EnsureSingleDefaultOptionPerSection(IList<ProductConfiguration> configurations)
    {
        if (configurations.IsNullOrEmpty())
        {
            return;
        }

        foreach (var option in configurations
                     .SelectMany(configuration => configuration?.Sections ?? [])
                     .SelectMany(section => section?.Options?.Where(option => option is { IsDefault: true }).Skip(1) ?? []))
        {
            option.IsDefault = false;
        }
    }
}
