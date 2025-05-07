using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CategoryService : CrudService<Category, CategoryEntity, CategoryChangingEvent, CategoryChangedEvent>, ICategoryService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IEventPublisher _eventPublisher;
        private readonly AbstractValidator<IHasProperties> _hasPropertyValidator;
        private readonly ICatalogService _catalogService;
        private readonly IOutlineService _outlineService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IPropertyValueSanitizer _propertyValueSanitizer;

        private readonly StringComparer _ignoreCase = StringComparer.OrdinalIgnoreCase;

        public CategoryService(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            AbstractValidator<IHasProperties> hasPropertyValidator,
            ICatalogService catalogService,
            IOutlineService outlineService,
            IBlobUrlResolver blobUrlResolver,
            IPropertyValueSanitizer propertyValueSanitizer)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
            _hasPropertyValidator = hasPropertyValidator;
            _catalogService = catalogService;
            _outlineService = outlineService;
            _blobUrlResolver = blobUrlResolver;
            _propertyValueSanitizer = propertyValueSanitizer;
        }

        public override Task<IList<Category>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
        {
            return GetByIdsAsync(ids, responseGroup, clone, catalogId: null);
        }

        public virtual Task<IList<Category>> GetByIdsAsync(IList<string> ids, string responseGroup, string catalogId)
        {
            return GetByIdsAsync(ids, responseGroup, clone: true, catalogId);
        }

        private async Task<IList<Category>> GetByIdsAsync(IList<string> ids, string responseGroup, bool clone, string catalogId)
        {
            var result = new List<Category>();

            ids = ids
                ?.Where(x => !string.IsNullOrEmpty(x))
                .Distinct(_ignoreCase)
                .ToArray();

            if (ids is null || ids.Count == 0)
            {
                return result;
            }

            var categoryById = await GetAllRelatedCategories(ids);

            foreach (var categoryId in ids)
            {
                if (!categoryById.TryGetValue(categoryId, out var category) || category is null)
                {
                    continue;
                }

                if (!clone)
                {
                    // Build all outlines
                    _outlineService.FillOutlinesForObjects(new List<Category> { category }, catalogId: null);
                }
                else
                {
                    category = category.CloneTyped();

                    if (HasFlag(responseGroup, CategoryResponseGroup.WithOutlines))
                    {
                        // Build outlines only for the requested catalog
                        _outlineService.FillOutlinesForObjects(new List<Category> { category }, catalogId);
                    }

                    // Reduce details according to response group
                    category.ReduceDetails(responseGroup);
                }

                result.Add(category);
            }

            return result;
        }

        public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            var categories = await GetAsync(ids, nameof(CategoryResponseGroup.Info));

            if (categories.Any())
            {
                var changedEntries = await GetDeletedEntries(categories);

                await _eventPublisher.Publish(new CategoryChangingEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangingEvent(changedEntries.ProductEntries));

                using (var repository = _repositoryFactory())
                {
                    // TODO: Implement soft delete
                    await repository.RemoveCategoriesAsync(ids);
                    await repository.UnitOfWork.CommitAsync();
                }

                await ClearCacheAsync(categories);

                await _eventPublisher.Publish(new CategoryChangedEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries.ProductEntries));
            }
        }


        protected override async Task BeforeSaveChanges(IList<Category> models)
        {
            await base.BeforeSaveChanges(models);
            await ValidateCategoryPropertiesAsync(models);
            SanitizeCategoryProperties(models);
        }

        protected override Task<IList<CategoryEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((ICatalogRepository)repository).GetCategoriesByIdsAsync(ids, responseGroup);
        }

        protected virtual bool HasFlag(string responseGroup, CategoryResponseGroup flag)
        {
            var categoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CategoryResponseGroup.Full);

            return categoryResponseGroup.HasFlag(flag);
        }

        private async Task<Dictionary<string, Category>> GetAllRelatedCategories(IList<string> ids)
        {
            var categoryById = new Dictionary<string, Category>();

            while (ids.Count > 0)
            {
                var entities = await GetOrLoadEntities(ids);

                foreach (var id in ids)
                {
                    var entity = entities.FirstOrDefault(x => x.Id.EqualsIgnoreCase(id));
                    var model = entity is null ? null : ToModel(entity);
                    categoryById.TryAdd(id, model);
                }

                var parentCategoryIds = entities
                    .Select(x => x.ParentCategoryId);

                var linkedCategoryIds = entities
                    .Where(x => x.OutgoingLinks?.Count > 0)
                    .SelectMany(x => x.OutgoingLinks.Select(y => y.TargetCategoryId));

                ids = parentCategoryIds.Concat(linkedCategoryIds)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct(_ignoreCase)
                    .Except(categoryById.Keys)
                    .ToArray();
            }

            var categories = categoryById.Values.Where(x => x != null).ToArray();

            ResolveImageUrls(categories);
            await LoadDependencies(categories, categoryById);
            ApplyInheritanceRules(categories);

            return categoryById;
        }

        private async Task<IList<CategoryEntity>> GetOrLoadEntities(IList<string> ids)
        {
            var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetOrLoadEntities));

            return await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, ids,
                async missingIds =>
                {
                    using var repository = _repositoryFactory();
                    repository.DisableChangesTracking();
                    return await LoadEntities(repository, missingIds, nameof(CategoryResponseGroup.Full));
                },
                ConfigureCache);
        }

        protected virtual Task<IDictionary<string, Category>> PreloadCategoryBranchAsync(string categoryId)
        {
            if (categoryId == null)
            {
                return Task.FromResult<IDictionary<string, Category>>(new Dictionary<string, Category>());
            }

            var cacheKey = CacheKey.With(GetType(), nameof(PreloadCategoryBranchAsync), categoryId);

            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheOptions =>
            {
                var entities = await SearchCategoriesHierarchyAsync(categoryId);

                IDictionary<string, Category> result = entities
                    .Select(x => x.ToModel(AbstractTypeFactory<Category>.TryCreateInstance()))
                    .ToDictionary(x => x.Id, _ignoreCase);

                // Prepare catalog cache tokens
                foreach (var entity in entities)
                {
                    ConfigureCache(cacheOptions, entity.Id, entity);
                }

                // Find linked category ids to recursively load them
                var linkedCategoryIds = entities
                    .SelectMany(x => x.OutgoingLinks.Select(y => y.TargetCategoryId))
                    .Where(x => x != null)
                    .Distinct()
                    .Except(result.Keys)
                    .ToList();

                foreach (var linkedCategoryId in linkedCategoryIds)
                {
                    // Recursive call
                    var linkedCategory = await PreloadCategoryBranchAsync(linkedCategoryId);

                    // Union two category sets (parents and linked)
                    foreach (var (key, value) in linkedCategory)
                    {
                        result.TryAdd(key, value);
                    }
                }

                ResolveImageUrls(result.Values);
                await LoadDependencies(result.Values, result);
                ApplyInheritanceRules(result.Values);

                return result;
            });
        }

        protected virtual void ConfigureCache(MemoryCacheEntryOptions cacheOptions, string id, CategoryEntity entity)
        {
            cacheOptions.AddExpirationToken(CatalogTreeCacheRegion.CreateChangeTokenForKey(id));

            if (entity != null)
            {
                cacheOptions.AddExpirationToken(CatalogTreeCacheRegion.CreateChangeTokenForKey(entity.CatalogId));
            }
        }

        protected virtual async Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            using var repository = _repositoryFactory();
            repository.DisableChangesTracking();

            return await LoadEntities(repository, [categoryId], nameof(CategoryResponseGroup.Full));
        }

        protected virtual void ResolveImageUrls(ICollection<Category> categories)
        {
            var allImages = new { categories }.GetFlatObjectsListWithInterface<IHasImages>().Where(x => x.Images != null).SelectMany(x => x.Images);
            foreach (var image in allImages.Where(x => !string.IsNullOrEmpty(x.Url)))
            {
                image.RelativeUrl = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;
                image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
            }
        }

        protected virtual async Task LoadDependencies(ICollection<Category> categories, IDictionary<string, Category> preloadedCategoriesMap)
        {
            var catalogIds = new { categories }.GetFlatObjectsListWithInterface<IHasCatalogId>()
                .Select(x => x.CatalogId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(_ignoreCase)
                .ToArray();

            var catalogsByIdDict = (await _catalogService.GetNoCloneAsync(catalogIds)).ToDictionary(x => x.Id, _ignoreCase);

            foreach (var category in categories)
            {
                category.Catalog = catalogsByIdDict.GetValueOrThrow(category.CatalogId, $"catalog with key {category.CatalogId} doesn't exist");
                category.IsVirtual = category.Catalog.IsVirtual;
                category.Parents = [];

                // Load all parent categories
                if (category.ParentId != null)
                {
                    category.Parents = GetParents(category, preloadedCategoriesMap).ToArray();
                    category.Parent = category.Parents.LastOrDefault();
                }

                category.Level = category.Parents.Length;

                foreach (var link in category.Links ?? [])
                {
                    link.Catalog = catalogsByIdDict.GetValueOrThrow(link.CatalogId, $"link catalog with key {link.CatalogId} doesn't exist");
                    if (link.CategoryId != null)
                    {
                        link.Category = preloadedCategoriesMap.GetValueSafe(link.CategoryId);
                    }
                }

                foreach (var property in category.Properties ?? [])
                {
                    property.Catalog = property.CatalogId != null ? catalogsByIdDict[property.CatalogId] : null;
                    if (property.CategoryId != null)
                    {
                        property.Category = preloadedCategoriesMap.GetValueSafe(property.CategoryId);
                    }
                }
            }
        }

        private static List<Category> GetParents(Category category, IDictionary<string, Category> preloadedCategoriesMap)
        {
            var list = new List<Category>();

            for (var parent = preloadedCategoriesMap.GetValueSafe(category.ParentId);
                 parent != null;
                 parent = preloadedCategoriesMap.GetValueSafe(parent.ParentId))
            {
                list.Insert(0, parent);
            }

            return list;
        }

        protected virtual void ApplyInheritanceRules(ICollection<Category> categories)
        {
            foreach (var category in categories.OrderBy(x => x.Level))
            {
                category.TryInheritFrom(category.Parent ?? (IEntity)category.Catalog);
            }
        }

        protected virtual async Task ValidateCategoryPropertiesAsync(IList<Category> categories)
        {
            ArgumentNullException.ThrowIfNull(categories);

            // Validate categories 
            var validator = new CategoryValidator();

            foreach (var category in categories)
            {
                await validator.ValidateAndThrowAsync(category);

                var branchId = category.Id ?? category.ParentId;
                if (string.IsNullOrEmpty(branchId))
                {
                    continue;
                }

                var group = new List<Category> { category };
                var categoryById = await GetAllRelatedCategories([branchId]);

                await LoadDependencies(group, categoryById);
                ApplyInheritanceRules(group);

                // PT-4999: fix validation call
                var validationResult = await _hasPropertyValidator.ValidateAsync(category);
                if (!validationResult.IsValid)
                {
                    throw new PlatformException($"Category properties has validation error: {string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ToString()))}");
                }
            }
        }

        protected virtual void SanitizeCategoryProperties(IList<Category> categories)
        {
            categories.SanitizePropertyValues(_propertyValueSanitizer);
        }

        protected override void ClearCache(IList<Category> models)
        {
            ClearCacheAsync(models).GetAwaiter().GetResult();
        }

        protected virtual async Task ClearCacheAsync(IEnumerable<Category> categories)
        {
            GenericSearchCachingRegion<Category>.ExpireRegion();

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var categoryIds = categories.Select(x => x.Id).ToList();

                // Find all linked categories first
                var linkedCategoryIds = await repository.CategoryLinks
                    .Where(x => categoryIds.Contains(x.TargetCategoryId))
                    .Select(x => x.SourceCategoryId)
                    .Distinct()
                    .ToListAsync();

                var categoryAndLinkedCategoryIds = categoryIds.Union(linkedCategoryIds).ToList();
                var childCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryAndLinkedCategoryIds);

                var allCategoryIds = categoryAndLinkedCategoryIds.Union(childCategoryIds);

                foreach (var categoryId in allCategoryIds)
                {
                    CatalogTreeCacheRegion.ExpireTokenForKey(categoryId, true);
                }
            }

            SeoInfoCacheRegion.ExpireRegion();
        }

        protected virtual async Task<CatalogChangedEntriesAggregate> GetDeletedEntries(IList<Category> categories)
        {
            using var repository = _repositoryFactory();

            var categoryIds = categories.Select(x => x.Id).ToList();
            var childCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds);

            var allCategoryIds = categoryIds.Concat(childCategoryIds).ToList();

            var productIds = await repository.Items
                .Where(x => allCategoryIds.Contains(x.CategoryId))
                .Select(x => x.Id)
                .ToListAsync();

            var categoryEntries = categories.Select(c => new GenericChangedEntry<Category>(c, EntryState.Deleted));
            var childCategoryEntries = childCategoryIds.Select(CreateDeletedEntry<Category>);
            var productEntries = productIds.Select(CreateDeletedEntry<CatalogProduct>).ToList();

            return new CatalogChangedEntriesAggregate
            {
                CategoryEntries = categoryEntries.Concat(childCategoryEntries).ToList(),
                ProductEntries = productEntries,
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
