using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Validation;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ItemService : IItemService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly AbstractValidator<IHasProperties> _hasPropertyValidator;
        private readonly ICatalogService _catalogService;
        private readonly ICategoryService _categoryService;
        private readonly IOutlineService _outlineService;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly ISkuGenerator _skuGenerator;

        public ItemService(
            Func<ICatalogRepository> catalogRepositoryFactory
            , IEventPublisher eventPublisher
            , AbstractValidator<IHasProperties> hasPropertyValidator
            , ICatalogService catalogService
            , ICategoryService categoryService
            , IOutlineService outlineService
            , IPlatformMemoryCache platformMemoryCache
            , IBlobUrlResolver blobUrlResolver
            , ISkuGenerator skuGenerator)
        {
            _repositoryFactory = catalogRepositoryFactory;
            _eventPublisher = eventPublisher;
            _hasPropertyValidator = hasPropertyValidator;
            _categoryService = categoryService;
            _outlineService = outlineService;
            _platformMemoryCache = platformMemoryCache;
            _blobUrlResolver = blobUrlResolver;
            _skuGenerator = skuGenerator;
            _catalogService = catalogService;
        }

        #region IItemService Members

        public virtual async Task<CatalogProduct[]> GetByIdsAsync(string[] itemIds, string respGroup, string catalogId = null)
        {
            var itemResponseGroup = EnumUtility.SafeParseFlags(respGroup, ItemResponseGroup.ItemLarge);

            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", itemIds), itemResponseGroup.ToString(), catalogId);
            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                var products = Array.Empty<CatalogProduct>();

                if (!itemIds.IsNullOrEmpty())
                {
                    using (var repository = _repositoryFactory())
                    {
                        //Optimize performance and CPU usage
                        repository.DisableChangesTracking();

                        //It is so important to generate change tokens for all ids even for not existing objects to prevent an issue
                        //with caching of empty results for non - existing objects that have the infinitive lifetime in the cache
                        //and future unavailability to create objects with these ids.
                        cacheEntry.AddExpirationToken(ItemCacheRegion.CreateChangeToken(itemIds));
                        cacheEntry.AddExpirationToken(CatalogCacheRegion.CreateChangeToken());

                        products = (await repository.GetItemByIdsAsync(itemIds, respGroup))
                            .Select(x => x.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance()))
                            .ToArray();
                    }

                    if (products.Any())
                    {
                        products = products.OrderBy(x => Array.IndexOf(itemIds, x.Id)).ToArray();
                        await LoadDependenciesAsync(products);
                        ApplyInheritanceRules(products);

                        var productsWithVariationsList = products.Concat(products.Where(p => p.Variations != null)
                            .SelectMany(p => p.Variations)).ToArray();

                        // Fill outlines for products and variations
                        if (itemResponseGroup.HasFlag(ItemResponseGroup.Outlines))
                        {
                            _outlineService.FillOutlinesForObjects(productsWithVariationsList, catalogId);
                        }
                        //Add change tokens for products with variations
                        cacheEntry.AddExpirationToken(ItemCacheRegion.CreateChangeToken(productsWithVariationsList));

                        //Reduce details according to response group
                        foreach (var product in productsWithVariationsList)
                        {
                            product.ReduceDetails(itemResponseGroup.ToString());
                        }
                    }
                }

                return products;
            });

            return result.Select(x => x.Clone() as CatalogProduct).ToArray();
        }

        public virtual async Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId = null)
        {
            CatalogProduct result = null;

            if (!string.IsNullOrEmpty(itemId))
            {
                var results = await GetByIdsAsync(new[] { itemId }, responseGroup, catalogId);
                result = results.Any() ? results.First() : null;
            }

            return result;
        }

        public virtual async Task SaveChangesAsync(CatalogProduct[] items)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<CatalogProduct>>();

            await ValidateProductsAsync(items);

            using (var repository = _repositoryFactory())
            {
                var dbExistProducts = await repository.GetItemByIdsAsync(items.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var product in items)
                {
                    var modifiedEntity = AbstractTypeFactory<ItemEntity>.TryCreateInstance().FromModel(product, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == product.Id);

                    if (originalEntity != null)
                    {
                        /// This extension is allow to get around breaking changes is introduced in EF Core 3.0 that leads to throw
                        /// Database operation expected to affect 1 row(s) but actually affected 0 row(s) exception when trying to add the new children entities with manually set keys
                        /// https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#detectchanges-honors-store-generated-key-values
                        repository.TrackModifiedAsAddedForNewChildEntities(originalEntity);

                        changedEntries.Add(new GenericChangedEntry<CatalogProduct>(product, originalEntity.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity.Patch(originalEntity);
                        //Force set ModifiedDate property to mark a product changed. Special for  partial update cases when product table not have changes
                        originalEntity.ModifiedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<CatalogProduct>(product, EntryState.Added));
                    }
                }

                await _eventPublisher.Publish(new ProductChangingEvent(changedEntries));

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                ClearCache(items);

                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries));
            }
        }

        public virtual async Task DeleteAsync(string[] itemIds)
        {
            var items = await GetByIdsAsync(itemIds, ItemResponseGroup.ItemInfo.ToString());
            var changedEntries = items
                .Select(i => new GenericChangedEntry<CatalogProduct>(i, EntryState.Deleted))
                .ToList();

            using (var repository = _repositoryFactory())
            {
                await _eventPublisher.Publish(new ProductChangingEvent(changedEntries));

                await repository.RemoveItemsAsync(itemIds);
                await repository.UnitOfWork.CommitAsync();

                ClearCache(items);

                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries));
            }
        }

        #endregion

        protected virtual void ClearCache(IEnumerable<CatalogProduct> entities)
        {
            AssociationSearchCacheRegion.ExpireRegion();
            SeoInfoCacheRegion.ExpireRegion();

            foreach (var entity in entities)
            {
                ItemCacheRegion.ExpireEntity(entity);
            }
        }

        public virtual async Task LoadDependenciesAsync(IEnumerable<CatalogProduct> products)
        {
            //TODO: refactor to do this by one call and iteration
            var catalogsIds = new { products }.GetFlatObjectsListWithInterface<IHasCatalogId>().Select(x => x.CatalogId).Where(x=> x != null).Distinct().ToArray();
            var catalogsByIdDict = (await _catalogService.GetByIdsAsync(catalogsIds)).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            var categoriesIds = new { products }.GetFlatObjectsListWithInterface<IHasCategoryId>().Select(x => x.CategoryId).Where(x => x != null).Distinct().ToArray();
            var categoriesByIdDict = (await _categoryService.GetByIdsAsync(categoriesIds, CategoryResponseGroup.Full.ToString())).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            var allImages = new { products }.GetFlatObjectsListWithInterface<IHasImages>().Where(x => x.Images != null).SelectMany(x => x.Images);
            foreach (var image in allImages.Where(x => !string.IsNullOrEmpty(x.Url)))
            {
                image.RelativeUrl = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;
                image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
            }

            foreach (var product in products)
            {
                SetProductDependencies(product, catalogsByIdDict, categoriesByIdDict);

                if (product.MainProduct != null)
                {
                    SetProductDependencies(product.MainProduct, catalogsByIdDict, categoriesByIdDict);
                }
                if (!product.Variations.IsNullOrEmpty())
                {
                    foreach (var variation in product.Variations)
                    {
                        SetProductDependencies(variation, catalogsByIdDict, categoriesByIdDict);
                    }
                }
            }
        }

        protected virtual void SetProductDependencies(CatalogProduct product, IDictionary<string, Catalog> catalogsByIdDict, IDictionary<string, Category> categoriesByIdDict)
        {
            //TOD: Refactor after cover by the unit tests
            if (string.IsNullOrEmpty(product.Code))
            {
                product.Code = _skuGenerator.GenerateSku(product);
            }
            product.Catalog = catalogsByIdDict.GetValueOrThrow(product.CatalogId, $"catalog with key {product.CatalogId} doesn't exist");
            if (product.CategoryId != null)
            {
                product.Category = categoriesByIdDict.GetValueOrThrow(product.CategoryId, $"category with key {product.CategoryId} doesn't exist");
            }

            foreach (var link in product.Links ?? Array.Empty<CategoryLink>())
            {
                link.Catalog = catalogsByIdDict.GetValueOrThrow(link.CatalogId, $"link catalog with key {link.CatalogId} doesn't exist");

                if (!string.IsNullOrEmpty(link.CategoryId))
                {
                    link.Category = categoriesByIdDict.GetValueOrThrow(link.CategoryId, $"link category with key {link.CategoryId} doesn't exist").Clone() as Category;
                    link.Category.ReduceDetails((CategoryResponseGroup.WithProperties | CategoryResponseGroup.WithParents).ToString());
                }
            }
        }

        protected virtual void ApplyInheritanceRules(IEnumerable<CatalogProduct> products)
        {
            foreach (var product in products)
            {
                if (product.MainProduct != null)
                {
                    //need to apply inheritance rules for main product first.
                    product.MainProduct.TryInheritFrom(product.Category ?? (IEntity)product.Catalog);
                    product.TryInheritFrom(product.MainProduct);
                }
                else
                {
                    product.TryInheritFrom(product.Category ?? (IEntity)product.Catalog);
                }
            }
        }

        protected virtual async Task ValidateProductsAsync(CatalogProduct[] products)
        {
            if (products == null)
            {
                throw new ArgumentNullException(nameof(products));
            }

            //Validate products
            var validator = new ProductValidator();
            foreach (var product in products)
            {
                validator.ValidateAndThrow(product);
            }

            await LoadDependenciesAsync(products);
            ApplyInheritanceRules(products);

            var targets = products.OfType<IHasProperties>();
            foreach (var item in targets)
            {
                var validationResult = await _hasPropertyValidator.ValidateAsync(item);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException($"Product properties has validation error: {string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ToString()))}");
                }
            }
        }
    }
}
