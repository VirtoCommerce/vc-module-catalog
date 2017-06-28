using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
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
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                result = repository.GetItemByIds(itemIds, respGroup)
                                   .Select(x => x.ToModel(AbstractTypeFactory<CatalogProduct>.TryCreateInstance()))
                                   .ToArray();
            }

            LoadProductDependencies(result);
            ApplyInheritanceRules(result);

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

            //Reduce details according to response group
            foreach (var product in result)
            {
                if (!respGroup.HasFlag(ItemResponseGroup.ItemAssets))
                {
                    product.Assets = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.ItemAssociations))
                {
                    product.Associations = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.ItemEditorialReviews))
                {
                    product.Reviews = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.ItemProperties))
                {
                    product.Properties = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.Links))
                {
                    product.Links = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.Outlines))
                {
                    product.Outlines = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.Seo))
                {
                    product.SeoInfos = null;
                }
                if (!respGroup.HasFlag(ItemResponseGroup.Variations))
                {
                    product.Variations = null;
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

        
        protected virtual void LoadProductDependencies(CatalogProduct[] products, bool processVariations = true)
        {
            var catalogsMap = _catalogService.GetCatalogsList().ToDictionary(x => x.Id);
            var allCategoriesIds = products.Select(x => x.CategoryId).Distinct().ToArray();
            var categoriesMap = _categoryService.GetByIds(allCategoriesIds, CategoryResponseGroup.Full).ToDictionary(x => x.Id);

            foreach (var product in products)
            {
                product.Catalog = catalogsMap[product.CatalogId];
                if (product.CategoryId != null)
                {
                    product.Category = categoriesMap[product.CategoryId];
                }

                if (product.Links != null)
                {
                    foreach (var link in product.Links)
                    {
                        link.Catalog = catalogsMap[link.CatalogId];
                        link.Category = _categoryService.GetById(link.CategoryId, CategoryResponseGroup.WithProperties);
                    }
                }

                if (product.MainProduct != null)
                {
                    LoadProductDependencies(new[] { product.MainProduct }, false);
                }
                if (processVariations && !product.Variations.IsNullOrEmpty())
                {
                    LoadProductDependencies(product.Variations.ToArray());
                }
            }
        }

        protected virtual void ApplyInheritanceRules(CatalogProduct[] products)
        {
            foreach (var product in products)
            {
                //Inherit images from parent product (if its not set)
                if (!product.Images.Any() && product.MainProduct != null && !product.MainProduct.Images.IsNullOrEmpty())
                {
                    product.Images = product.MainProduct.Images.Select(x => x.Clone()).OfType<Image>().ToList();
                    foreach (var image in product.Images)
                    {
                        image.Id = null;
                        image.IsInherited = true;
                    }
                }

                //Inherit assets from parent product (if its not set)
                if (!product.Assets.Any() && product.MainProduct != null && product.MainProduct.Assets != null)
                {
                    product.Assets = product.MainProduct.Assets.Select(x => x.Clone()).OfType<Asset>().ToList();
                    foreach (var asset in product.Assets)
                    {
                        asset.Id = null;
                        asset.IsInherited = true;
                    }
                }

                //inherit editorial reviews from main product and do not inherit if variation loaded within product
                if (!product.Reviews.Any() && product.MainProduct != null && product.MainProduct.Reviews != null)
                {
                    product.Reviews = product.MainProduct.Reviews.Select(x => x.Clone()).OfType<EditorialReview>().ToList();
                    foreach (var review in product.Reviews)
                    {
                        review.Id = null;
                        review.IsInherited = true;
                    }
                }

                //TaxType category inheritance
                if (product.TaxType == null && product.Category != null)
                {
                    product.TaxType = product.Category.TaxType;
                }

                //Properties inheritance
                product.Properties = (product.Category != null ? product.Category.Properties : product.Catalog.Properties).Select(x => x.Clone())
                                     .OfType<Property>()
                                     .OrderBy(x => x.Name)
                                     .ToList();
                foreach (var property in product.Properties)
                {
                    property.IsInherited = true;
                }

                //Self item property values
                foreach (var propertyValue in product.PropertyValues.ToArray())
                {
                    //Try to find property meta information
                    propertyValue.Property = product.Properties.FirstOrDefault(x => x.IsSuitableForValue(propertyValue));
                    //Return each localized value for selected dictionary value
                    //Because multilingual dictionary values for all languages may not stored in db need add it in result manually from property dictionary values
                    var localizedDictValues = propertyValue.TryGetAllLocalizedDictValues();
                    foreach (var localizedDictValue in localizedDictValues)
                    {
                        if (!product.PropertyValues.Any(x => x.ValueId == localizedDictValue.ValueId && x.LanguageCode == localizedDictValue.LanguageCode))
                        {
                            product.PropertyValues.Add(localizedDictValue);
                        }
                    }
                }

                //inherit not overriden property values from main product
                if (product.MainProduct != null && product.MainProduct.PropertyValues != null)
                {
                    var mainProductPopValuesGroups = product.MainProduct.PropertyValues.GroupBy(x => x.PropertyName);
                    foreach (var group in mainProductPopValuesGroups)
                    {
                        //Inherit all values if not overriden
                        if (!product.PropertyValues.Any(x => x.PropertyName.EqualsInvariant(group.Key)))
                        {
                            foreach (var inheritedpropValue in group)
                            {
                                inheritedpropValue.Id = null;
                                inheritedpropValue.IsInherited = true;
                                product.PropertyValues.Add(inheritedpropValue);
                            }
                        }
                    }
                }
                //Measurement inheritance 
                if(product.MainProduct != null)
                {
                    product.Width = product.Width ?? product.MainProduct.Width;
                    product.Height = product.Height ?? product.MainProduct.Height;
                    product.Length = product.Length ?? product.MainProduct.Length;
                    product.MeasureUnit = product.MeasureUnit ?? product.MainProduct.MeasureUnit;
                    product.Weight = product.Weight ?? product.MainProduct.Weight;
                    product.WeightUnit = product.WeightUnit ?? product.MainProduct.WeightUnit;
                    product.PackageType = product.PackageType ?? product.MainProduct.PackageType;
                }

                if (!product.Variations.IsNullOrEmpty())
                {
                    ApplyInheritanceRules(product.Variations.ToArray());
                }
            }
        }
    }
}
