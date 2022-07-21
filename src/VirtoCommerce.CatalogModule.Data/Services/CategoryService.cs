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
    public class CategoryService : CrudService<Category, CategoryEntity, CategoryChangingEvent, CategoryChangedEvent>, ICategoryCrudService, ICategoryService
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

        #region ICategoryService compatibility

        public virtual async Task<Category[]> GetByIdsAsync(string[] categoryIds, string responseGroup, string catalogId = null)
        {
            return (await GetAsync(categoryIds, responseGroup, catalogId)).ToArray();
        }

        public virtual Task SaveChangesAsync(Category[] categories)
        {
            return SaveChangesAsync(categories.AsEnumerable());
        }

        public virtual Task DeleteAsync(string[] categoryIds)
        {
            return DeleteAsync(categoryIds, softDelete: false);
        }

        #endregion

        public override async Task<IReadOnlyCollection<Category>> GetAsync(List<string> ids, string responseGroup = null)
        {
            return (IReadOnlyCollection<Category>)await GetAsync(ids, responseGroup, catalogId: null);
        }

        public virtual async Task<Category> GetAsync(string id, string responseGroup, string catalogId)
        {
            var categories = await GetAsync(new[] { id }, responseGroup, catalogId);

            return categories.FirstOrDefault();
        }

        public virtual async Task<IList<Category>> GetAsync(IList<string> ids, string responseGroup, string catalogId)
        {
            var result = new List<Category>();

            foreach (var categoryId in ids.Where(x => x != null))
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

        public override async Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            var categoryIds = ids.ToArray();
            var categories = await GetByIdsAsync(categoryIds, CategoryResponseGroup.Info.ToString(), catalogId: null);

            var changedEntries = await GetDeletedEntries(categories);

            using (var repository = _repositoryFactory())
            {
                await _eventPublisher.Publish(new CategoryChangingEvent(changedEntries.CategoryEntries));
                await _eventPublisher.Publish(new ProductChangingEvent(changedEntries.ProductEntries));

                // TODO: Implement soft delete
                await repository.RemoveCategoriesAsync(categoryIds);
                await repository.UnitOfWork.CommitAsync();

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

        protected Task<IDictionary<string, Category>> PreloadCategoryBranchAsync(string categoryId)
        {
            if (categoryId == null)
            {
                return Task.FromResult<IDictionary<string, Category>>(new Dictionary<string, Category>());
            }

            var cacheKey = CacheKey.With(GetType(), "PreloadCategoryBranch", categoryId);

            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CatalogTreeCacheRegion.CreateChangeTokenForKey(categoryId));

                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var entities = await repository.SearchCategoriesHierarchyAsync(categoryId);

                    var result = entities.Select(x => x.ToModel(AbstractTypeFactory<Category>.TryCreateInstance()))
                        .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
                        .WithDefaultValue(null);

                    // Prepare catalog cache tokens
                    foreach (var catalogId in result.Values.Select(x => x.CatalogId).Distinct())
                    {
                        cacheEntry.AddExpirationToken(CatalogTreeCacheRegion.CreateChangeTokenForKey(catalogId));
                    }

                    // Find link category ids to recursievly load them
                    var linkedCategoryIds = result.Values.SelectMany(x => x.Links.Select(x => x.CategoryId)).Where(x => x != null).Distinct().ToList();
                    linkedCategoryIds.RemoveAll(x => result.ContainsKey(x));

                    foreach (var linkedCategoryId in linkedCategoryIds)
                    {
                        // Recursive call
                        var linkedCategory = await PreloadCategoryBranchAsync(linkedCategoryId);

                        // Union two category sets (parents and linked)
                        result.AddRange(linkedCategory);
                    }

                    ResolveImageUrls(result.Values);

                    await LoadDependencies(result.Values, result);

                    ApplyInheritanceRules(result.Values);

                    return result;
                }
            });
        }

        [Obsolete("Use PreloadCategoriesAsync() instead.")]
        protected virtual async Task<IDictionary<string, Category>> PreloadCategoriesAsync(string catalogId)
        {
            return await PreloadCategoriesAsync();
        }

        protected Task<IDictionary<string, Category>> PreloadCategoriesAsync()
        {
            var cacheKey = CacheKey.With(GetType(), "PreloadCategories");
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
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

                var result = entities.Select(x => x.ToModel(AbstractTypeFactory<Category>.TryCreateInstance()))
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
            var catalogsIds = new { categories }.GetFlatObjectsListWithInterface<IHasCatalogId>().Select(x => x.CatalogId).Where(x => x != null).Distinct().ToArray();
            var catalogsByIdDict = (await _catalogService.GetByIdsAsync(catalogsIds)).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var category in categories)
            {
                category.Catalog = catalogsByIdDict.GetValueOrThrow(category.CatalogId, $"catalog with key {category.CatalogId} doesn't exist");
                category.IsVirtual = category.Catalog.IsVirtual;
                category.Parents = Array.Empty<Category>();

                // Load all parent categories
                if (category.ParentId != null)
                {
                    category.Parents = TreeExtension.GetAncestors(category, x => x.ParentId != null ? preloadedCategoriesMap[x.ParentId] : null)
                                                    .Reverse()
                                                    .ToArray();
                    category.Parent = category.Parents.LastOrDefault();
                }
                category.Level = category.Parents?.Count() ?? 0;

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
                validator.ValidateAndThrow(category);

                var group = new List<Category>() { category };

                var branchId = category.Id ?? category.ParentId;
                var categoryBranch = await PreloadCategoryBranchAsync(branchId);

                await LoadDependencies(group, categoryBranch);
                ApplyInheritanceRules(group);

                // PT-4999: fix validation call
                var validationResult = _hasPropertyValidator.Validate(category);
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

        private async Task ClearCacheAsync(IEnumerable<Category> categories)
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

                categoryIds = categoryIds.Union(linkedCategoryIds).ToArray();

                var childrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(categoryIds);
                var allCategoryIds = categoryIds.Union(childrenCategoryIds);

                foreach (var categoryId in allCategoryIds)
                {
                    CatalogTreeCacheRegion.ExpireTokenForKey(categoryId, true);
                }
            }

            SeoInfoCacheRegion.ExpireRegion();
        }

        private async Task<CatalogChangedEntriesAggregate> GetDeletedEntries(IList<Category> categories)
        {
            using (var repository = _repositoryFactory())
            {
                var deletedCategoryIds = categories.Select(x => x.Id).ToList();
                var deletedChildrenCategoryIds = await repository.GetAllChildrenCategoriesIdsAsync(deletedCategoryIds.ToArray());

                deletedCategoryIds.AddRange(deletedChildrenCategoryIds);
                var deletedChildrenProductIds = await repository.Items.Where(x => deletedCategoryIds.Contains(x.CategoryId)).Select(x => x.Id).ToListAsync();

                var deletedCategoryEntries = deletedChildrenCategoryIds.Select(id =>
                {
                    var category = AbstractTypeFactory<Category>.TryCreateInstance();
                    category.Id = id;
                    var entry = new GenericChangedEntry<Category>(category, EntryState.Deleted);

                    return entry;
                }).ToList();

                var deletedProductEntries = deletedChildrenProductIds.Select(id =>
                {
                    var product = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();
                    product.Id = id;
                    var entry = new GenericChangedEntry<CatalogProduct>(product, EntryState.Deleted);

                    return entry;
                }).ToList();

                deletedCategoryEntries.AddRange(categories
                    .Select(c => new GenericChangedEntry<Category>(c, EntryState.Deleted)));

                return new CatalogChangedEntriesAggregate
                {
                    CategoryEntries = deletedCategoryEntries,
                    ProductEntries = deletedProductEntries,
                };
            }
        }
    }
}
