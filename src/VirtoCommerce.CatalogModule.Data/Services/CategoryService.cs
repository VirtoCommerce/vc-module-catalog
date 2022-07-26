using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Validation;
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
        private new readonly IPlatformMemoryCache _platformMemoryCache;
        private new readonly Func<ICatalogRepository> _repositoryFactory;
        private new readonly IEventPublisher _eventPublisher;
        private readonly AbstractValidator<IHasProperties> _hasPropertyValidator;
        private readonly ICatalogService _catalogService;
        private readonly IOutlineService _outlineService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public CategoryService(
            Func<ICatalogRepository> catalogRepositoryFactory
            , IEventPublisher eventPublisher
            , IPlatformMemoryCache platformMemoryCache
            , AbstractValidator<IHasProperties> hasPropertyValidator
            , ICatalogService catalogService
            , IOutlineService outlineService
            , IBlobUrlResolver blobUrlResolver)
            : base(catalogRepositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = catalogRepositoryFactory;
            _eventPublisher = eventPublisher;
            _platformMemoryCache = platformMemoryCache;
            _hasPropertyValidator = hasPropertyValidator;
            _outlineService = outlineService;
            _blobUrlResolver = blobUrlResolver;
            _catalogService = catalogService;
        }

        #region ICategoryService Members

        public virtual async Task<Category[]> GetByIdsAsync(string[] categoryIds, string responseGroup, string catalogId)
        {
            var result = new List<Category>();

            foreach (var categoryId in categoryIds.Where(x => x != null))
            {
                var categoryBranch = await PreloadCategoryBranchAsync(categoryId);
                var category = categoryBranch[categoryId];

                if (category != null)
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

            return result.ToArray();
        }

        #endregion

        public override async Task<IReadOnlyCollection<Category>> GetAsync(List<string> ids, string responseGroup = null)
        {
            return await GetByIdsAsync(ids.ToArray(), responseGroup, catalogId: null);
        }

        public override async Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            var categoryIds = ids.ToArray();
            var categories = await GetByIdsAsync(categoryIds, CategoryResponseGroup.Info.ToString(), catalogId: null);

            if (categories.Any())
            {
                var changedEntries = await GetDeletedEntries(categories);

                await _eventPublisher.Publish(new CategoryChangingEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangingEvent(changedEntries.ProductEntries));

                using (var repository = _repositoryFactory())
                {
                    // TODO: Implement soft delete
                    await repository.RemoveCategoriesAsync(categoryIds);
                    await repository.UnitOfWork.CommitAsync();
                }

                await ClearCacheAsync(categories);

                await _eventPublisher.Publish(new CategoryChangedEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries.ProductEntries));
            }
        }


        protected override async Task BeforeSaveChanges(IEnumerable<Category> models)
        {
            var categories = models.ToArray();
            await base.BeforeSaveChanges(categories);
            await ValidateCategoryPropertiesAsync(categories);
        }

        protected override async Task<IEnumerable<CategoryEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            var entities = await ((ICatalogRepository)repository).GetCategoriesByIdsAsync(ids.ToArray(), responseGroup);

            return entities;
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

        protected virtual async Task<ICollection<CategoryEntity>> SearchCategoriesHierarchyAsync(string categoryId)
        {
            using var repository = _repositoryFactory();
            repository.DisableChangesTracking();

            return await repository.SearchCategoriesHierarchyAsync(categoryId);
        }

        [Obsolete("Use PreloadCategoriesAsync() instead.")]
        protected virtual async Task<IDictionary<string, Category>> PreloadCategoriesAsync(string catalogId)
        {
            return await PreloadCategoriesAsync();
        }

        [Obsolete("Use PreloadCategoryBranchAsync()")]
        protected Task<IDictionary<string, Category>> PreloadCategoriesAsync()
        {
            var cacheKey = CacheKey.With(GetType(), nameof(PreloadCategoriesAsync));
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());

                var entities = new List<CategoryEntity>();
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();
                    var categoriesIds = repository.Categories
                        .Select(x => x.Id)
                        .ToArray();

                    foreach (var page in categoriesIds.Paginate(50))
                    {
                        entities.AddRange(await repository.GetCategoriesByIdsAsync(page.ToArray(), CategoryResponseGroup.Full.ToString()));
                    }
                }

                var result = entities
                    .Select(x => x.ToModel(AbstractTypeFactory<Category>.TryCreateInstance()))
                    .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .WithDefaultValue(null);

                // Resolve relative URLs for all category assets
                ResolveImageUrls(result.Values);
                await LoadDependencies(result.Values, result);
                ApplyInheritanceRules(result.Values);

                return result;
            });
        }

        protected virtual void ResolveImageUrls(IEnumerable<Category> categories)
        {
            var allImages = new { categories }.GetFlatObjectsListWithInterface<IHasImages>().Where(x => x.Images != null).SelectMany(x => x.Images);
            foreach (var image in allImages.Where(x => !string.IsNullOrEmpty(x.Url)))
            {
                image.RelativeUrl = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;
                image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
            }
        }

        [Obsolete("Use LoadDependencies()")]
        protected virtual Task LoadDependenciesAsync(IEnumerable<Category> categories, IDictionary<string, Category> preloadedCategoriesMap)
        {
            return LoadDependencies(categories.ToList(), preloadedCategoriesMap);
        }

        protected virtual async Task LoadDependencies(ICollection<Category> categories, IDictionary<string, Category> preloadedCategoriesMap)
        {
            var catalogsIds = new { categories }.GetFlatObjectsListWithInterface<IHasCatalogId>().Select(x => x.CatalogId).Where(x => x != null).Distinct().ToList();
            var catalogsByIdDict = (await _catalogService.GetAsync(catalogsIds)).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var category in categories)
            {
                category.Catalog = catalogsByIdDict.GetValueOrThrow(category.CatalogId, $"catalog with key {category.CatalogId} doesn't exist");
                category.IsVirtual = category.Catalog.IsVirtual;
                category.Parents = Array.Empty<Category>();

                // Load all parent categories
                if (category.ParentId != null)
                {
                    category.Parents = TreeExtension
                        .GetAncestors(category, x => x.ParentId != null ? preloadedCategoriesMap[x.ParentId] : null)
                        .Reverse()
                        .ToArray();

                    category.Parent = category.Parents.LastOrDefault();
                }

                category.Level = category.Parents?.Length ?? 0;

                foreach (var link in category.Links ?? Array.Empty<CategoryLink>())
                {
                    link.Catalog = catalogsByIdDict.GetValueOrThrow(link.CatalogId, $"link catalog with key {link.CatalogId} doesn't exist");
                    if (link.CategoryId != null)
                    {
                        link.Category = preloadedCategoriesMap[link.CategoryId];
                    }
                }

                foreach (var property in category.Properties ?? Array.Empty<Property>())
                {
                    property.Catalog = property.CatalogId != null ? catalogsByIdDict[property.CatalogId] : null;
                    if (property.CategoryId != null)
                    {
                        property.Category = preloadedCategoriesMap[property.CategoryId];
                    }
                }
            }
        }

        protected virtual void ApplyInheritanceRules(IEnumerable<Category> categories)
        {
            foreach (var category in categories.OrderBy(x => x.Level))
            {
                category.TryInheritFrom(category.Parent ?? (IEntity)category.Catalog);
            }
        }

        protected virtual async Task ValidateCategoryPropertiesAsync(IEnumerable<Category> categories)
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

        protected override void ClearCache(IEnumerable<Category> models)
        {
            ClearCacheAsync(models).GetAwaiter().GetResult();
        }

        protected virtual async Task ClearCacheAsync(IEnumerable<Category> categories)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var categoryIds = categories.Select(x => x.Id).ToArray();

                // Find all linked categories first
                var linkedCategoryIds = await repository.CategoryLinks
                    .Where(x => categoryIds.Contains(x.TargetCategoryId))
                    .Select(x => x.SourceCategoryId)
                    .Distinct()
                    .ToArrayAsync();

                var categoryAndLinkedCategoryIds = categoryIds.Union(linkedCategoryIds).ToArray();
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

            var categoryIds = categories.Select(x => x.Id).ToArray();
            var childCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds);

            var allCategoryIds = categoryIds.Concat(childCategoryIds).ToArray();

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
