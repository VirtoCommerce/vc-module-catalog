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
        private readonly ISanitizerService _sanitizerService;

        public CategoryService(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            AbstractValidator<IHasProperties> hasPropertyValidator,
            ICatalogService catalogService,
            IOutlineService outlineService,
            IBlobUrlResolver blobUrlResolver,
            ISanitizerService sanitizerService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
            _hasPropertyValidator = hasPropertyValidator;
            _catalogService = catalogService;
            _outlineService = outlineService;
            _blobUrlResolver = blobUrlResolver;
            _sanitizerService = sanitizerService;
        }

        public override Task<IList<Category>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
        {
            return clone
                ? GetByIdsAsync(ids, responseGroup, catalogId: null)
                : GetNoCloneAsync(ids, responseGroup);
        }

        public virtual async Task<IList<Category>> GetByIdsAsync(IList<string> ids, string responseGroup, string catalogId)
        {
            var result = new List<Category>();

            foreach (var categoryId in ids.Where(x => x != null))
            {
                var categoryBranch = await PreloadCategoryBranchAsync(categoryId);

                if (categoryBranch.TryGetValue(categoryId, out var category) && category != null)
                {
                    category = category.CloneTyped();

                    if (HasFlag(responseGroup, CategoryResponseGroup.WithOutlines))
                    {
                        _outlineService.FillOutlinesForObjects(new List<Category> { category }, catalogId);
                    }

                    // Reduce details according to response group
                    category.ReduceDetails(responseGroup);

                    result.Add(category);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns data from the cache without cloning. This consumes less memory, but returned data must not be modified.
        /// </summary>
        protected virtual async Task<IList<Category>> GetNoCloneAsync(IList<string> ids, string responseGroup)
        {
            var result = new List<Category>();

            foreach (var categoryId in ids.Where(x => x != null))
            {
                var categoryBranch = await PreloadCategoryBranchAsync(categoryId);

                if (categoryBranch.TryGetValue(categoryId, out var category) && category != null)
                {
                    _outlineService.FillOutlinesForObjects(new List<Category> { category }, catalogId: null);
                    result.Add(category);
                }
            }

            return result;
        }

        public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            var categories = await GetAsync(ids, CategoryResponseGroup.Info.ToString());

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

        protected virtual Task<IDictionary<string, Category>> PreloadCategoryBranchAsync(string categoryId)
        {
            if (categoryId == null)
            {
                return Task.FromResult<IDictionary<string, Category>>(new Dictionary<string, Category>());
            }

            var cacheKey = CacheKey.With(GetType(), nameof(PreloadCategoryBranchAsync), categoryId);

            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(CatalogTreeCacheRegion.CreateChangeTokenForKey(categoryId));

                var entities = await SearchCategoriesHierarchyAsync(categoryId);

                var result = entities
                    .Select(x => x.ToModel(AbstractTypeFactory<Category>.TryCreateInstance()))
                    .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .WithDefaultValue(null);

                // Prepare catalog cache tokens
                foreach (var catalogId in entities.Select(x => x.CatalogId).Distinct())
                {
                    cacheEntry.AddExpirationToken(CatalogTreeCacheRegion.CreateChangeTokenForKey(catalogId));
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

        protected virtual async Task<IList<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            using var repository = _repositoryFactory();
            repository.DisableChangesTracking();

            return await LoadEntities(repository, new[] { categoryId }, CategoryResponseGroup.Full.ToString());
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
            var catalogsIds = new { categories }.GetFlatObjectsListWithInterface<IHasCatalogId>().Select(x => x.CatalogId).Where(x => x != null).Distinct().ToList();
            var catalogsByIdDict = (await _catalogService.GetNoCloneAsync(catalogsIds)).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var category in categories)
            {
                category.Catalog = catalogsByIdDict.GetValueOrThrow(category.CatalogId, $"catalog with key {category.CatalogId} doesn't exist");
                category.IsVirtual = category.Catalog.IsVirtual;
                category.Parents = Array.Empty<Category>();

                // Load all parent categories
                if (category.ParentId != null)
                {
                    category.Parents = (await GetParentsAsync(category, preloadedCategoriesMap)).ToArray();
                    category.Parent = category.Parents.LastOrDefault();
                }

                category.Level = category.Parents?.Length ?? 0;

                foreach (var link in category.Links ?? Array.Empty<CategoryLink>())
                {
                    link.Catalog = catalogsByIdDict.GetValueOrThrow(link.CatalogId, $"link catalog with key {link.CatalogId} doesn't exist");
                    if (link.CategoryId != null)
                    {
                        link.Category = await GetCategoryAsync(link.CategoryId, preloadedCategoriesMap);
                    }
                }

                foreach (var property in category.Properties ?? Array.Empty<Property>())
                {
                    property.Catalog = property.CatalogId != null ? catalogsByIdDict[property.CatalogId] : null;
                    if (property.CategoryId != null)
                    {
                        property.Category = await GetCategoryAsync(property.CategoryId, preloadedCategoriesMap);
                    }
                }
            }
        }

        private async Task<IList<Category>> GetParentsAsync(Category category, IDictionary<string, Category> preloadedCategoriesMap)
        {
            var list = new List<Category>();

            for (var parent = await GetCategoryAsync(category.ParentId, preloadedCategoriesMap);
                 parent != null;
                 parent = await GetCategoryAsync(parent.ParentId, preloadedCategoriesMap))
            {
                list.Insert(0, parent);
            }

            return list;
        }

        private async Task<Category> GetCategoryAsync(string id, IDictionary<string, Category> preloadedCategoriesMap)
        {
            if (id is null)
            {
                return null;
            }

            if (preloadedCategoriesMap.TryGetValue(id, out var result) && result != null)
            {
                return result;
            }

            result = (await PreloadCategoryBranchAsync(id))[id];
            preloadedCategoriesMap[id] = result;

            return result;
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
            if (categories == null)
            {
                throw new ArgumentNullException(nameof(categories));
            }

            // Validate categories 
            var validator = new CategoryValidator();

            foreach (var category in categories)
            {
                await validator.ValidateAndThrowAsync(category);

                var group = new List<Category> { category };

                var branchId = category.Id ?? category.ParentId;
                var categoryBranch = await PreloadCategoryBranchAsync(branchId);

                await LoadDependencies(group, categoryBranch);
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
            categories.OfType<IHasProperties>().SanitizeProperties(_sanitizerService);
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
