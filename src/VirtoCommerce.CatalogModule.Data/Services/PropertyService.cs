using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyService : CrudService<Property, PropertyEntity, PropertyChangingEvent, PropertyChangedEvent>, IPropertyService
    {
        private readonly SearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog, CatalogEntity> _catalogSearchService;
        private readonly AbstractValidator<Property> _propertyValidator;

        public PropertyService(Func<ICatalogRepositoryForCrud> repositoryFactory,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            ICatalogSearchService catalogSearchService,
            AbstractValidator<Property> propertyValidator)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _catalogSearchService = (SearchService<CatalogSearchCriteria, CatalogSearchResult, Catalog, CatalogEntity>)catalogSearchService;
            _propertyValidator = propertyValidator;
        }

        public override async Task<IEnumerable<Property>> GetByIdsAsync(IEnumerable<string> ids, string responseGroup = null)
        {
            var preloadedProperties = await PreloadAllPropertiesAsync();

            var result = ids
                .Select(x => preloadedProperties[x])
                .Where(x => x != null)
                .Select(x => x.Clone()).OfType<Property>()
                .ToList();

            return result;
        }

        public override async Task DeleteAsync(IEnumerable<string> ids, bool doDeleteValues = false)
        {
            using (var repository = _repositoryFactory())
            {
                var entities = await ((ICatalogRepositoryForCrud)repository).GetPropertiesByIdsAsync(ids.ToArray());
                //Raise domain events before deletion
                var changedEntries = entities.Select(x => new GenericChangedEntry<Property>(x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance()), EntryState.Deleted));

                await _eventPublisher.Publish(new PropertyChangingEvent(changedEntries));

                foreach (var entity in entities)
                {
                    repository.Remove(entity);
                    if (doDeleteValues)
                    {
                        await ((ICatalogRepositoryForCrud)repository).RemoveAllPropertyValuesAsync(entity.Id);
                    }
                }
                await repository.UnitOfWork.CommitAsync();

                //Reset catalog cache
                CatalogCacheRegion.ExpireRegion();

                await _eventPublisher.Publish(new PropertyChangedEvent(changedEntries));
            }
        }

        protected override void ClearCache(IEnumerable<Property> models)
        {
            CatalogCacheRegion.ExpireRegion();
        }

        protected override Task<IEnumerable<PropertyEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((ICatalogRepositoryForCrud)repository).GetPropertiesByIdsAsync(ids);
        }

        #region IPropertyService members

        public async Task<IEnumerable<Property>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await base.GetByIdsAsync(ids);
        }

        public async Task<IEnumerable<Property>> GetAllCatalogPropertiesAsync(string catalogId)
        {
            var preloadedProperties = await PreloadAllCatalogPropertiesAsync(catalogId);
            var result = preloadedProperties.Select(x => x.Clone()).OfType<Property>().ToArray();
            return result;
        }

        #endregion IPropertyService members

        protected virtual Task<IDictionary<string, Property>> PreloadAllPropertiesAsync()
        {
            var cacheKey = CacheKey.With(GetType(), "PreloadAllProperties");
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());

                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var propertyIds = await ((ICatalogRepositoryForCrud)repository).Properties.Select(p => p.Id).ToArrayAsync();
                    var entities = await ((ICatalogRepositoryForCrud)repository).GetPropertiesByIdsAsync(propertyIds);
                    var properties = entities.Select(p => p.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToArray();

                    await LoadDependenciesAsync(properties);
                    ApplyInheritanceRules(properties);

                    var result = properties.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase).WithDefaultValue(null);
                    return result;
                }
            });
        }

        protected virtual Task<Property[]> PreloadAllCatalogPropertiesAsync(string catalogId)
        {
            var cacheKey = CacheKey.With(GetType(), "PreloadAllCatalogProperties", catalogId);
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var result = (await ((ICatalogRepositoryForCrud)repository).GetAllCatalogPropertiesAsync(catalogId))
                        .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase) // Remove duplicates
                        .Select(g => g.First())
                        .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(p => p.ToModel(AbstractTypeFactory<Property>.TryCreateInstance()))
                        .ToArray();

                    return result;
                }
            });
        }

        protected virtual async Task LoadDependenciesAsync(Property[] properties)
        {
            var catalogsByIdDict = ((await _catalogSearchService.SearchAsync(new Core.Model.Search.CatalogSearchCriteria { Take = int.MaxValue })).Results).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
                                                                           .WithDefaultValue(null);
            foreach (var property in properties.Where(x => x.CatalogId != null))
            {
                property.Catalog = catalogsByIdDict[property.CatalogId];
            }
        }

        protected virtual void ApplyInheritanceRules(Property[] properties)
        {
            foreach (var property in properties)
            {
                property.TryInheritFrom(property.Catalog);
            }
        }

        protected virtual void TryAddPredefinedValidationRules(IEnumerable<Property> properties)
        {
            foreach (var property in properties)
            {
                if (property.ValueType == PropertyValueType.GeoPoint)
                {
                    var geoPointValidationRule = property.ValidationRules?.FirstOrDefault(x => x.RegExp.EqualsInvariant(GeoPoint.Regexp.ToString()));
                    if (geoPointValidationRule == null)
                    {
                        if (property.ValidationRules == null)
                        {
                            property.ValidationRules = new List<PropertyValidationRule>();
                        }
                        property.ValidationRules.Add(new PropertyValidationRule { RegExp = GeoPoint.Regexp.ToString() });
                    }
                }
            }
        }

        protected virtual void ValidateProperties(IEnumerable<Property> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            foreach (var property in properties)
            {
                _propertyValidator.ValidateAndThrow(property);
            }
        }
    }
}
