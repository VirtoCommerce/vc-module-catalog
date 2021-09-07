using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogService : CrudService<Catalog, CatalogEntity, CatalogChangingEvent, CatalogChangedEvent>, ICatalogService
    {
        private readonly AbstractValidator<IHasProperties> _hasPropertyValidator;

        public CatalogService(Func<ICatalogRepository> repositoryFactory,
                                  IEventPublisher eventPublisher, IPlatformMemoryCache platformMemoryCache, AbstractValidator<IHasProperties> hasPropertyValidator)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _hasPropertyValidator = hasPropertyValidator;
        }

        public override async Task<IEnumerable<Catalog>> GetByIdsAsync(IEnumerable<string> catalogIds, string responseGroup = null)
        {
            var catalogResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CatalogResponseGroup.Full);

            var result = new List<Catalog>();
            var preloadedCatalogsByIdDict = await PreloadCatalogsAsync();
            foreach (var catalogId in catalogIds.Where(x => x != null))
            {
                var catalog = preloadedCatalogsByIdDict[catalogId];
                if (catalog != null)
                {
                    catalog = catalog.Clone() as Catalog;
                    //Reduce details according to response group
                    catalog.ReduceDetails(catalogResponseGroup.ToString());
                    result.Add(catalog);
                }
            }
            return result.ToArray();
        }

        public override async Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            using (var repository = _repositoryFactory())
            {
                var catalogs = await GetByIdsAsync(ids);
                if (!catalogs.IsNullOrEmpty())
                {
                    var changedEntries = catalogs.Select(x => new GenericChangedEntry<Catalog>(x, EntryState.Deleted));
                    await _eventPublisher.Publish(new CatalogChangingEvent(changedEntries));

                    await ((ICatalogRepository)repository).RemoveCatalogsAsync(catalogs.Select(m => m.Id).ToArray());
                    await repository.UnitOfWork.CommitAsync();

                    CatalogCacheRegion.ExpireRegion();

                    await _eventPublisher.Publish(new CatalogChangedEvent(changedEntries));
                }
            }
        }

        protected override Task<IEnumerable<CatalogEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((ICatalogRepository)repository).GetCatalogsByIdsAsync(ids);
        }

        protected override Task BeforeSaveChanges(IEnumerable<Catalog> models)
        {
            return ValidateCatalogPropertiesAsync(models);
        }

        protected override void ClearCache(IEnumerable<Catalog> models)
        {
            base.ClearCache(models);
            CatalogCacheRegion.ExpireRegion();
        }

        protected virtual Task<IDictionary<string, Catalog>> PreloadCatalogsAsync()
        {
            var cacheKey = CacheKey.With(GetType(), nameof(PreloadCatalogsAsync));
            return _platformMemoryCache.GetOrCreateExclusive(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());
                IEnumerable<CatalogEntity> entities;
                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    var ids = await ((ICatalogRepository)repository).Catalogs.Select(x => x.Id).ToListAsync();
                    entities = await ((ICatalogRepository)repository).GetCatalogsByIdsAsync(ids);
                }

                var result = entities.Select(x => x.ToModel(AbstractTypeFactory<Catalog>.TryCreateInstance()))
                                     .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
                                     .WithDefaultValue(null);

                LoadDependencies(result.Values, result);
                return result;
            });
        }

        protected virtual void LoadDependencies(IEnumerable<Catalog> catalogs, IDictionary<string, Catalog> preloadedCatalogsMap)
        {
            if (catalogs == null)
            {
                throw new ArgumentNullException(nameof(catalogs));
            }
            foreach (var catalog in catalogs.Where(x => !x.IsTransient()))
            {
                var preloadedCatalog = preloadedCatalogsMap[catalog.Id];
                if (preloadedCatalog != null)
                {
                    if (catalog.Properties.IsNullOrEmpty())
                    {
                        catalog.Properties = preloadedCatalog.Properties;
                    }
                    foreach (var property in catalog.Properties)
                    {
                        property.Catalog = preloadedCatalogsMap[property.CatalogId];
                    }
                }
            }
        }

        private async Task ValidateCatalogPropertiesAsync(IEnumerable<Catalog> catalogs)
        {
            LoadDependencies(catalogs, await PreloadCatalogsAsync());
            foreach (var catalog in catalogs)
            {
                var validatioResult = await _hasPropertyValidator.ValidateAsync(catalog);
                if (!validatioResult.IsValid)
                {
                    throw new ArgumentException($"Catalog properties has validation error: {string.Join(Environment.NewLine, validatioResult.Errors.Select(x => x.ToString()))}");
                }
            }
        }

        #region ICatalogService compatibility
        public async Task<Catalog[]> GetByIdsAsync(string[] catalogIds, string responseGroup = null)
        {
            return (await GetByIdsAsync(catalogIds.AsEnumerable(), responseGroup)).ToArray();
        }

        public Task SaveChangesAsync(Catalog[] catalogs)
        {
            return base.SaveChangesAsync(catalogs.AsEnumerable());
        }

        public Task DeleteAsync(string[] catalogIds)
        {
            return base.DeleteAsync(catalogIds.AsEnumerable());
        }
        #endregion
    }
}
