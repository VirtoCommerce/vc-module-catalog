using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly AbstractValidator<Property> _propertyValidator;
        private readonly IPropertyValueSanitizer _propertyValueSanitizer;

        public PropertyService(Func<ICatalogRepository> repositoryFactory,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            ICatalogSearchService catalogSearchService,
            AbstractValidator<Property> propertyValidator,
            IPropertyValueSanitizer propertyValueSanitizer)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
            _platformMemoryCache = platformMemoryCache;
            _catalogSearchService = catalogSearchService;
            _propertyValidator = propertyValidator;
            _propertyValueSanitizer = propertyValueSanitizer;
        }

        #region IPropertyService members

        public async Task<IEnumerable<Property>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var preloadedProperties = await PreloadAllPropertiesAsync();

            var result = ids
                .Select(x => preloadedProperties[x])
                .Where(x => x != null)
                .Select(x => x.Clone()).OfType<Property>()
                .ToList();

            return result;
        }

        public async Task<IEnumerable<Property>> GetAllCatalogPropertiesAsync(string catalogId)
        {
            var preloadedProperties = await PreloadAllCatalogPropertiesAsync(catalogId);
            var result = preloadedProperties.Select(x => x.Clone()).OfType<Property>().ToArray();
            return result;
        }

        public async Task SaveChangesAsync(IEnumerable<Property> properties)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<Property>>();

            ValidateProperties(properties);
            SanitizeProperties(properties);

            using (var repository = _repositoryFactory())
            {
                TryAddPredefinedValidationRules(properties);

                var dbExistProperties = await repository.GetPropertiesByIdsAsync(properties.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var property in properties)
                {
                    var modifiedEntity = AbstractTypeFactory<PropertyEntity>.TryCreateInstance().FromModel(property, pkMap);
                    var originalEntity = dbExistProperties.FirstOrDefault(x => x.Id == property.Id);

                    if (originalEntity != null)
                    {
                        repository.TrackModifiedAsAddedForNewChildEntities(originalEntity);

                        changedEntries.Add(new GenericChangedEntry<Property>(property, originalEntity.ToModel(AbstractTypeFactory<Property>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                        //Force set ModifiedDate property to mark a product changed. Special for  partial update cases when product table not have changes
                        originalEntity.ModifiedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<Property>(property, EntryState.Added));
                    }
                }

                //Raise domain events
                await _eventPublisher.Publish(new PropertyChangingEvent(changedEntries));
                //Save changes in database
                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                //Reset catalog cache
                ClearCache(properties);

                await _eventPublisher.Publish(new PropertyChangedEvent(changedEntries));
            }
        }

        // This method prevents switching to CrudService<>
        public async Task DeleteAsync(IEnumerable<string> ids, bool doDeleteValues = false)
        {
            using (var repository = _repositoryFactory())
            {
                var entities = await repository.GetPropertiesByIdsAsync(ids.ToArray());
                //Raise domain events before deletion
                var changedEntries = entities.Select(x => new GenericChangedEntry<Property>(x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance()), EntryState.Deleted));

                await _eventPublisher.Publish(new PropertyChangingEvent(changedEntries));

                foreach (var entity in entities)
                {
                    repository.Remove(entity);
                    if (doDeleteValues)
                    {
                        await repository.RemoveAllPropertyValuesAsync(entity.Id);
                    }
                }
                await repository.UnitOfWork.CommitAsync();

                //Reset catalog cache
                ClearCache(changedEntries.Select(x => x.NewEntry));

                await _eventPublisher.Publish(new PropertyChangedEvent(changedEntries));
            }
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

                    var propertyIds = await repository.Properties.Select(p => p.Id).ToArrayAsync();
                    var entities = await repository.GetPropertiesByIdsAsync(propertyIds);
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

                    var result = (await repository.GetAllCatalogPropertiesAsync(catalogId))
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
            var catalogsByIdDict = (await _catalogSearchService.SearchAsync(new Core.Model.Search.CatalogSearchCriteria { Take = int.MaxValue })).Results
                .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
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
                    var geoPointValidationRule = property.ValidationRules?.FirstOrDefault(x => x.RegExp.EqualsIgnoreCase(GeoPoint.Regexp.ToString()));
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

        protected virtual void SanitizeProperties(IEnumerable<Property> properties)
        {
            properties.SanitizeValues(_propertyValueSanitizer);
        }

        private void ClearCache(IEnumerable<Property> properties)
        {
            ClearCacheAsync(properties).GetAwaiter().GetResult();
        }

        private async Task ClearCacheAsync(IEnumerable<Property> properties)
        {
            CatalogCacheRegion.ExpireRegion();

            var categoryIds = new List<string>();
            var catalogIds = new List<string>();

            foreach (var property in properties)
            {
                // Look for 'categoryId' first becasue properties that are created on catalog level don't have 'categoryId' set
                // and we can clean catalog tree cache by 'catalogId' without looking into categories hierarchy 
                if (property.CategoryId != null)
                {
                    categoryIds.Add(property.CategoryId);
                }
                else if (property.CatalogId != null)
                {
                    catalogIds.Add(property.CatalogId);
                }
            }

            if (catalogIds.Any())
            {
                foreach (var catalogId in catalogIds.Distinct())
                {
                    CatalogTreeCacheRegion.ExpireTokenForKey(catalogId);
                }
            }

            if (categoryIds.Any())
            {
                using (var repository = _repositoryFactory())
                {
                    var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds.ToArray());
                    var allCategoryIds = categoryIds.Union(childrenCategoryIds).Distinct();

                    foreach (var categoryId in allCategoryIds)
                    {
                        CatalogTreeCacheRegion.ExpireTokenForKey(categoryId, true);
                    }
                }
            }
        }
    }
}
