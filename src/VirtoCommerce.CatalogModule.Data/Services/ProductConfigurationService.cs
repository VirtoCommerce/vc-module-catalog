using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    private readonly Func<ICatalogRepository> _repositoryFactory;
    private readonly IBlobUrlResolver _blobUrlResolver;

    public ProductConfigurationService(
        Func<ICatalogRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IBlobUrlResolver blobUrlResolver)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
        _blobUrlResolver = blobUrlResolver;
    }

    public virtual async Task SaveChangesAsync(ProductConfiguration configuration, CancellationToken cancellationToken)
    {
        // Only the full configuration can be active
        if ((configuration.Sections is null or []) || configuration.Sections.Any(x => x.Options is null or []))
        {
            configuration.IsActive = false;
        }

        await base.SaveChangesAsync([configuration]);

        using var repository = _repositoryFactory();

        await DeleteSectionsAsync(configuration, repository, cancellationToken);

        foreach (var section in configuration.Sections)
        {
            await DeleteOptionsAsync(section, repository, cancellationToken);
        }
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


    private static async Task DeleteSectionsAsync(ProductConfiguration configuration, ICatalogRepository repository, CancellationToken cancellationToken)
    {
        var sectionIds = configuration.Sections.Where(x => !x.IsTransient()).Select(x => x.Id).ToList();
        var deletedSections = await repository.ProductConfigurationSections
            .Where(x => x.ConfigurationId == configuration.Id && !sectionIds.Contains(x.Id))
            .Include(x => x.Options)
            .ToListAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var section in deletedSections)
        {
            foreach (var option in section.Options)
            {
                repository.Remove(option);
            }

            repository.Remove(section);
        }

        await repository.UnitOfWork.CommitAsync();
    }

    private static async Task DeleteOptionsAsync(ProductConfigurationSection section, ICatalogRepository repository, CancellationToken cancellationToken)
    {
        var optionIds = section.Options.Where(x => !x.IsTransient()).Select(x => x.Id).ToList();
        var deletedOptions = await repository.ProductConfigurationOptions.Where(x => x.SectionId == section.Id && !optionIds.Contains(x.Id)).ToListAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var option in deletedOptions)
        {
            repository.Remove(option);
        }

        await repository.UnitOfWork.CommitAsync();
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
