using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;


namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class ItemServiceImpl : ServiceBase, IItemService
    {
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;
        private readonly ICommerceService _commerceService;
        private readonly IOutlineService _outlineService;
        private readonly Func<ICatalogRepository> _repositoryFactory;
        public ItemServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICommerceService commerceService, IOutlineService outlineService, ICatalogService catalogService, ICategoryService categoryService, ICacheManager<object> cacheManager)
        {
            _catalogService = catalogService;
            _categoryService = categoryService;
            _commerceService = commerceService;
            _outlineService = outlineService;
            _repositoryFactory = catalogRepositoryFactory;
        }

        #region IItemService Members

        public CatalogProduct GetById(string itemId, ItemResponseGroup respGroup, string catalogId = null)
        {
            var results = this.GetByIds(new[] { itemId }, respGroup, catalogId);
            return results.Any() ? results.First() : null;
        }

        public CatalogProduct[] GetByIds(string[] itemIds, ItemResponseGroup respGroup, string catalogId = null)
        {
            CatalogProduct[] result;

            using (var repository = _repositoryFactory())
            {
                result = repository.GetItemByIds(itemIds, respGroup)
                                   .Select(x => x.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance()))
                                   .ToArray();
            }

            LoadProductDependencies(result);
           
            

            // Fill outlines for products
            if (respGroup.HasFlag(ItemResponseGroup.Outlines))
            {
                _outlineService.FillOutlinesForObjects(result, catalogId);
            }

            // Fill SEO info for products, variations and outline items
            if ((respGroup & ItemResponseGroup.Seo) == ItemResponseGroup.Seo)
            {
                var objectsWithSeo = new List<ISeoSupport>(result);

                var variations = result.Where(p => p.Variations != null)
                                       .SelectMany(p => p.Variations);
                objectsWithSeo.AddRange(variations);

                var outlineItems = result.Where(p => p.Outlines != null)
                                         .SelectMany(p => p.Outlines.SelectMany(o => o.Items));
                objectsWithSeo.AddRange(outlineItems);

                _commerceService.LoadSeoForObjects(objectsWithSeo.ToArray());
            }

            //Cleanup result model considered requested response group
            foreach (var product in result)
            {
                if (!respGroup.HasFlag(ItemResponseGroup.ItemProperties))
                {
                    product.Properties = null;
                }
            }

            return result;
        }

        public void Create(CatalogProduct[] items)
        {
            SaveChanges(items);
        }

        public CatalogProduct Create(CatalogProduct item)
        {
            Create(new[] { item });
            var retVal = GetById(item.Id, ItemResponseGroup.ItemLarge);
            return retVal;
        }

        public void Update(CatalogProduct[] items)
        {
            SaveChanges(items);
        }

        public void Delete(string[] itemIds)
        {
            var items = GetByIds(itemIds, ItemResponseGroup.Seo | ItemResponseGroup.Variations);
            using (var repository = _repositoryFactory())
            {
                repository.RemoveItems(itemIds);
                CommitChanges(repository);
            }
        }
        #endregion

        protected virtual void SaveChanges(CatalogProduct[] products, bool disableValidation = false)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbExistProducts = repository.GetItemByIds(products.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray(), Domain.Catalog.Model.ItemResponseGroup.ItemLarge);
                foreach (var product in products)
                {
                    var modifiedEntity = AbstractTypeFactory<ItemEntity>.TryCreateInstance().FromModel(product, pkMap);
                    var originalEntity = dbExistProducts.FirstOrDefault(x => x.Id == product.Id);
                    if (originalEntity != null)
                    {
                        changeTracker.Attach(originalEntity);
                        modifiedEntity.Patch(originalEntity);
                        //Force set ModifiedDate property to mark a product changed. Special for  partial update cases when product table not have changes
                        originalEntity.ModifiedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }

            //Update SEO 
            var productsWithVariations = products.Concat(products.Where(x => x.Variations != null).SelectMany(x => x.Variations)).ToArray();
            _commerceService.UpsertSeoForObjects(productsWithVariations);
        }

        
        protected virtual void LoadProductDependencies(CatalogProduct[] products)
        {
            foreach (var product in products)
            {
                product.Catalog = _catalogService.GetById(product.CatalogId);
                if (product.CategoryId != null)
                {
                    product.Category = _categoryService.GetById(product.CategoryId, CategoryResponseGroup.Info);
                }

                foreach (var link in product.Links)
                {
                    link.Catalog = _catalogService.GetById(link.CatalogId);
                    link.Category = _categoryService.GetById(link.CategoryId, CategoryResponseGroup.Info);
                }

                foreach (var property in product.Properties)
                {
                    property.Catalog = _catalogService.GetById(property.CatalogId);
                    if (property.CategoryId != null)
                    {
                        property.Category = _categoryService.GetById(property.CategoryId, CategoryResponseGroup.Info);
                    }
                }
                LoadProductDependencies(product.Variations.ToArray());              
            }
        }
    }
}
