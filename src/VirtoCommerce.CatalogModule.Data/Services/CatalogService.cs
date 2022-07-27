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
        private new readonly IPlatformMemoryCache _platformMemoryCache;
        private new readonly Func<ICatalogRepository> _repositoryFactory;
        private new readonly IEventPublisher _eventPublisher;
        private readonly AbstractValidator<IHasProperties> _hasPropertyValidator;

        public CatalogService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            AbstractValidator<IHasProperties> hasPropertyValidator)
            : base(catalogRepositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = catalogRepositoryFactory;
            _eventPublisher = eventPublisher;
            _platformMemoryCache = platformMemoryCache;
            _hasPropertyValidator = hasPropertyValidator;
        }

        #region ICatalogService Members

        public virtual async Task<Catalog[]> GetByIdsAsync(string[] catalogIds, string responseGroup = null)
        {
            return (await GetAsync(catalogIds.ToList(), responseGroup)).ToArray();
        }

        public virtual Task SaveChangesAsync(Catalog[] catalogs)
        {
            return SaveChangesAsync(catalogs.AsEnumerable());
        }

        public virtual Task DeleteAsync(string[] catalogIds)
        {
            return DeleteAsync(catalogIds, softDelete: false);
        }

        #endregion

        public override async Task<IReadOnlyCollection<Catalog>> GetAsync(List<string> ids, string responseGroup = null)
        {
            var result = new List<Catalog>();
            var catalogsByIds = await PreloadCatalogsAsync();

            foreach (var catalogId in ids.Where(x => x != null))
            {
                var catalog = catalogsByIds[catalogId];
                if (catalog != null)
                {
                    catalog = catalog.CloneTyped();

                    // Reduce details according to response group
                    catalog.ReduceDetails(responseGroup);

                    result.Add(catalog);
                }
            }

            return result;
        }

        public override async Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            var catalogs = await GetAsync(ids.ToList());

            if (catalogs.Any())
            {
                var changedEntries = await GetDeletedEntriesAsync(catalogs);

                await _eventPublisher.Publish(new CatalogChangingEvent(changedEntries.CatalogEntries));
                await _eventPublisher.Publish(new CategoryChangingEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangingEvent(changedEntries.ProductEntries));

                using (var repository = _repositoryFactory())
                {
                    // TODO: Implement soft delete
                    await repository.RemoveCatalogsAsync(catalogs.Select(m => m.Id).ToArray());
                    await repository.UnitOfWork.CommitAsync();
                }

                ClearCache(catalogs);

                await _eventPublisher.Publish(new CatalogChangedEvent(changedEntries.CatalogEntries));
                await _eventPublisher.Publish(new CategoryChangedEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries.ProductEntries));
            }
        }


        protected override async Task BeforeSaveChanges(IEnumerable<Catalog> models)
        {
            var catalogs = models.ToArray();
            await base.BeforeSaveChanges(catalogs);
            await ValidateCatalogPropertiesAsync(catalogs);
        }

        protected override async Task<IEnumerable<CatalogEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            var entities = await ((ICatalogRepository)repository).GetCatalogsByIdsAsync(ids.ToArray());

            return entities;
        }

        protected virtual Task<IDictionary<string, Catalog>> PreloadCatalogsAsync()
        {
            var cacheKey = CacheKey.With(GetType(), nameof(PreloadCatalogsAsync));

            return _platformMemoryCache.GetOrCreateExclusive(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());
                CatalogEntity[] entities;

                using (var repository = _repositoryFactory())
                {
                    // Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    var ids = await repository.Catalogs.Select(x => x.Id).ToArrayAsync();
                    entities = await repository.GetCatalogsByIdsAsync(ids);
                }

                var result = entities
                    .Select(x => x.ToModel(AbstractTypeFactory<Catalog>.TryCreateInstance()))
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

        protected virtual async Task ValidateCatalogPropertiesAsync(IList<Catalog> catalogs)
        {
            LoadDependencies(catalogs, await PreloadCatalogsAsync());

            foreach (var catalog in catalogs)
            {
                var validationResult = await _hasPropertyValidator.ValidateAsync(catalog);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException($"Catalog properties has validation error: {string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ToString()))}");
                }
            }
        }

        protected override void ClearCache(IEnumerable<Catalog> models)
        {
            CatalogCacheRegion.ExpireRegion();

            foreach (var catalog in models)
            {
                CatalogTreeCacheRegion.ExpireTokenForKey(catalog.Id);
            }
        }

        protected virtual async Task<CatalogChangedEntriesAggregate> GetDeletedEntriesAsync(IReadOnlyCollection<Catalog> catalogs)
        {
            using var repository = _repositoryFactory();

            var catalogIds = catalogs.Select(x => x.Id).ToList();
            var categoryIds = await repository.Categories.Where(x => catalogIds.Contains(x.CatalogId)).Select(x => x.Id).ToListAsync();
            var productIds = await repository.Items.Where(x => catalogIds.Contains(x.CatalogId)).Select(x => x.Id).ToListAsync();

            return new CatalogChangedEntriesAggregate
            {
                CatalogEntries = catalogs.Select(x => new GenericChangedEntry<Catalog>(x, EntryState.Deleted)).ToList(),
                CategoryEntries = categoryIds.Select(CreateDeletedEntry<Category>).ToList(),
                ProductEntries = productIds.Select(CreateDeletedEntry<CatalogProduct>).ToList(),
            };
        }

        protected virtual GenericChangedEntry<T> CreateDeletedEntry<T>(string id)
            where T : IEntity
        {
            var entity = AbstractTypeFactory<T>.TryCreateInstance();
            entity.Id = id;

            var entry = new GenericChangedEntry<T>(entity, EntryState.Deleted);

            return entry;
        }
    }
}
