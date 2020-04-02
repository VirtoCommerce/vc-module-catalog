using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
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
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICategoryService _categoryService;
        private readonly IOutlineService _outlineService;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly ISkuGenerator _skuGenerator;

        public ItemService(
            Func<ICatalogRepository> catalogRepositoryFactory
            , IEventPublisher eventPublisher
            , AbstractValidator<IHasProperties> hasPropertyValidator
            , ICatalogSearchService catalogSearchService
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
            _catalogSearchService = catalogSearchService;
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

        public virtual async Task SaveChangesAsync(CatalogProduct[] products)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<CatalogProduct>>();

            await ValidateProductsAsync(products);

            using (var repository = _repositoryFactory())
            {
                var dbExistProducts = await repository.GetItemByIdsAsync(products.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var product in products)
                {
                    var modifiedEntity = AbstractTypeFactory<ItemEntity>.TryCreateInstance().FromModel(product, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == product.Id);

                    if (originalEntity != null)
                    {
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

                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries));
            }

            ClearCache(products);
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

                await _eventPublisher.Publish(new ProductChangedEvent(changedEntries));
            }

            ClearCache(items);
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
            await InnerLoadDependenciesAsync(products);
        }

        protected virtual async Task InnerLoadDependenciesAsync(IEnumerable<CatalogProduct> products, bool processVariations = true)
        {
            var catalogsByIdDict = ((await _catalogSearchService.SearchCatalogsAsync(new Core.Model.Search.CatalogSearchCriteria { Take = int.MaxValue })).Results).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
                                                                            .WithDefaultValue(null);
            var productsCategoryIds = products.Select(x => x.CategoryId)
                                           .Where(x => x != null).Distinct()
                                           .ToArray();
            var productLinksCategoryIds = products.Any(p => !p.Links.IsNullOrEmpty()) ? products.SelectMany(p => p.Links)
                                                  .Select(l => l.CategoryId).Distinct().ToArray() : new string[] { };

            var productCategoriesByIdDict = (await _categoryService.GetByIdsAsync(productsCategoryIds, CategoryResponseGroup.Full.ToString())).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase).WithDefaultValue(null);
            var productLinksCategoriesByIdDict = (await _categoryService.GetByIdsAsync(productLinksCategoryIds, (CategoryResponseGroup.WithProperties | CategoryResponseGroup.WithParents).ToString())).ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase).WithDefaultValue(null);

            foreach (var product in products)
            {
                if (string.IsNullOrEmpty(product.Code))
                {
                    product.Code = _skuGenerator.GenerateSku(product);
                }
                product.Catalog = catalogsByIdDict.GetValueOrThrow(product.CatalogId, $"catalog with key {product.CatalogId} doesn't exist");
                if (product.CategoryId != null)
                {
                    product.Category = productCategoriesByIdDict.GetValueOrThrow(product.CategoryId, $"category with key {product.CategoryId} doesn't exist");
                }

                foreach (var link in product.Links ?? Array.Empty<CategoryLink>())
                {
                    link.Catalog = catalogsByIdDict.GetValueOrThrow(link.CatalogId, $"link catalog with key {link.CatalogId} doesn't exist");

                    if (!string.IsNullOrEmpty(link.CategoryId))
                    {
                        link.Category = productLinksCategoriesByIdDict.GetValueOrThrow(link.CategoryId, $"link category with key {link.CategoryId} doesn't exist");
                    }
                }

                if (product.MainProduct != null)
                {
                    await InnerLoadDependenciesAsync(new[] { product.MainProduct }, false);
                }
                if (processVariations && !product.Variations.IsNullOrEmpty())
                {
                    await InnerLoadDependenciesAsync(product.Variations.ToArray());
                }

                //Resolve relative urls for all product images
                var allImages = product.GetFlatObjectsListWithInterface<IHasImages>().Where(x => x.Images != null).SelectMany(x => x.Images);
                foreach (var image in allImages.Where(x => !string.IsNullOrEmpty(x.Url)))
                {
                    image.RelativeUrl = !string.IsNullOrEmpty(image.RelativeUrl) ? image.RelativeUrl : image.Url;
                    image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
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

            await InnerLoadDependenciesAsync(products, false);
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
