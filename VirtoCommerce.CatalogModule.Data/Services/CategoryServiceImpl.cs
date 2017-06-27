using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using Omu.ValueInjecter;
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
    public class CategoryServiceImpl : ServiceBase, ICategoryService
    {
        private readonly ICommerceService _commerceService;
        private readonly IOutlineService _outlineService;
        private readonly ICacheManager<object> _cacheManager;
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly ICatalogService _catalogService;
        public CategoryServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICommerceService commerceService, IOutlineService outlineService, ICatalogService catalogService, ICacheManager<object> cacheManager)
        {
            _repositoryFactory = catalogRepositoryFactory;
            _cacheManager = cacheManager;
            _commerceService = commerceService;
            _outlineService = outlineService;
            _catalogService = catalogService;
        }

        #region ICategoryService Members
        public virtual Category[] GetByIds(string[] categoryIds, CategoryResponseGroup responseGroup, string catalogId = null)
        {
            var result = PreloadCategories(catalogId).Where(x => categoryIds.Contains(x.Id))
                                                     .Select(x => MemberwiseCloneCategory(x))
                                                     .ToArray();

            //Reduce details according to response group
            foreach (var category in result)
            {
                if (!responseGroup.HasFlag(CategoryResponseGroup.WithImages))
                {
                    category.Images = null;
                }
                if (!responseGroup.HasFlag(CategoryResponseGroup.WithLinks))
                {
                    category.Links = null;
                }
                if (!responseGroup.HasFlag(CategoryResponseGroup.WithParents))
                {
                    category.Parents = null;
                }
                if (!responseGroup.HasFlag(CategoryResponseGroup.WithProperties))
                {
                    category.Properties = null;
                }
                if (!responseGroup.HasFlag(CategoryResponseGroup.WithOutlines))
                {
                    category.Outlines = null;
                }
                if (!responseGroup.HasFlag(CategoryResponseGroup.WithSeo))
                {
                    category.SeoInfos = null;
                }
            }

            return result;
        }

        public virtual Category GetById(string categoryId, CategoryResponseGroup responseGroup, string catalogId = null)
        {
            return GetByIds(new[] { categoryId }, responseGroup, catalogId).FirstOrDefault();
        }

        public virtual void Create(Category[] categories)
        {
            if (categories == null)
                throw new ArgumentNullException("categories");

            SaveChanges(categories);
        }


        public virtual Category Create(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            Create(new[] { category });
            return GetById(category.Id, Domain.Catalog.Model.CategoryResponseGroup.Info);
        }

        public virtual void Update(Category[] categories)
        {
            SaveChanges(categories);
        }

        public virtual void Delete(string[] categoryIds)
        {
            var categories = GetByIds(categoryIds, CategoryResponseGroup.WithSeo);
            using (var repository = _repositoryFactory())
            {
                repository.RemoveCategories(categoryIds);
                CommitChanges(repository);
                //Reset cached categories and catalogs
                ResetCache();
            }
        }

        #endregion

        protected virtual void SaveChanges(Category[] categories)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbExistCategories = repository.GetCategoriesByIds(categories.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray(), Domain.Catalog.Model.CategoryResponseGroup.Full);
                foreach (var category in categories)
                {
                    var originalEntity = dbExistCategories.FirstOrDefault(x => x.Id == category.Id);
                    var modifiedEntity = AbstractTypeFactory<CategoryEntity>.TryCreateInstance().FromModel(category, pkMap);
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
                //Reset cached categories and catalogs
                ResetCache();
            }
            //Need add seo separately
            _commerceService.UpsertSeoForObjects(categories);
        }
      
        protected virtual void ResetCache()
        {
            _cacheManager.ClearRegion(CatalogConstants.CacheRegion);
        }

        //TODO: need to move in domain
        protected virtual Category MemberwiseCloneCategory(Category category)
        {
            var retVal = AbstractTypeFactory<Category>.TryCreateInstance();

            retVal.Id = category.Id;
            retVal.CatalogId = category.CatalogId;
            retVal.Children = category.Children;
            retVal.Code = category.Code;
            retVal.CreatedBy = category.CreatedBy;
            retVal.CreatedDate = category.CreatedDate;
            retVal.IsActive = category.IsActive;
            retVal.IsVirtual = category.IsVirtual;
            retVal.Level = category.Level;
            retVal.ModifiedBy = category.ModifiedBy;
            retVal.ModifiedDate = category.ModifiedDate;
            retVal.Name = category.Name;
            retVal.PackageType = category.PackageType;
            retVal.ParentId = category.ParentId;
            retVal.Path = category.Path;
            retVal.Priority = category.Priority;
            retVal.TaxType = category.TaxType;

            //Set all reference properties from preloaded category
            retVal.Outlines = category.Outlines;
            retVal.PropertyValues = category.PropertyValues;
            retVal.SeoInfos = category.SeoInfos;
            retVal.Catalog = category.Catalog;
            retVal.Properties = category.Properties;
            retVal.Parents = category.Parents;
            retVal.Links = category.Links;
            retVal.Images = category.Images;
            retVal.Children = category.Children;
            return retVal;
        }

        protected virtual Category[] PreloadCategories(string catalogId)
        {
            return _cacheManager.Get($"AllCategories-{catalogId}", CatalogConstants.CacheRegion, () =>
            {
                var result = new List<Category>();
                CategoryEntity[] entities = null;
                using (var repository = _repositoryFactory())
                {
                    //EF multi-thread issue for cached entities
                    //http://stackoverflow.com/questions/29106477/nullreferenceexception-in-entity-framework-from-trygetcachedrelatedend
                    if (repository is System.Data.Entity.DbContext)
                    {
                        var dbConfiguration = ((System.Data.Entity.DbContext)repository).Configuration;
                        dbConfiguration.ProxyCreationEnabled = false;
                        dbConfiguration.AutoDetectChangesEnabled = false;
                    }
                    entities = repository.GetCategoriesByIds(repository.Categories.Select(x => x.Id).ToArray(), Domain.Catalog.Model.CategoryResponseGroup.Full);
                }

                var catalogsMap = _catalogService.GetCatalogsList().ToDictionary(x => x.Id);

                foreach (var entity in entities.OrderBy(x => x.AllParents.Count()))
                {
                    var allParents = new List<Category>();
                    var category = entity.ToModel(AbstractTypeFactory<Category>.TryCreateInstance());
                    foreach (var parent in entity.AllParents)
                    {
                        allParents.Add(result.First(x => x.Id == parent.Id));
                    }
                    category.Catalog = catalogsMap[category.CatalogId];
                    category.IsVirtual = category.Catalog.IsVirtual;
                    category.Parents = allParents.ToArray();
                    category.Level = category.Parents.Count();
                    //Try to inherit taxType from parent category
                    if (category.TaxType == null && category.Parents != null)
                    {
                        category.TaxType = category.Parents.Select(x => x.TaxType).Where(x => x != null).FirstOrDefault();
                    }
                    //Inherit properties
                    var properties = category.Catalog.Properties.ToList();
                    //For parents categories                       
                    properties.AddRange(category.Parents.SelectMany(x => x.Properties));
                    // Self properties
                    properties.AddRange(category.Properties);

                    //property override - need leave only property has a min distance to target category 
                    //Algorithm based on index property in resulting list (property with min index will more closed to category)
                    category.Properties = properties.Select((x, index) => new { PropertyName = x.Name.ToLowerInvariant(), Property = x, Index = index })
                                               .GroupBy(x => x.PropertyName)
                                               .Select(x => x.OrderBy(y => y.Index).First().Property)
                                               .OrderBy(x => x.Name)
                                               .ToList();

                    //Next need set Property in PropertyValues objects
                    foreach (var propValue in category.PropertyValues.ToArray())
                    {
                        propValue.Property = category.Properties.FirstOrDefault(x => x.IsSuitableForValue(propValue));
                        //Because multilingual dictionary values for all languages may not stored in db then need to add it in result manually from property dictionary values
                        var localizedDictValues = propValue.TryGetAllLocalizedDictValues();
                        foreach (var localizedDictValue in localizedDictValues)
                        {
                            if (!category.PropertyValues.Any(x => x.ValueId == localizedDictValue.ValueId && x.LanguageCode == localizedDictValue.LanguageCode))
                            {
                                category.PropertyValues.Add(localizedDictValue);
                            }
                        }
                    }
                    result.Add(category);
                }

                foreach (var link in result.SelectMany(x => x.Links))
                {
                    link.Catalog = catalogsMap[link.CatalogId];
                    link.Category = result.First(x => x.Id == link.CategoryId);
                }

                foreach (var property in result.SelectMany(x => x.Properties).Distinct())
                {
                    property.Catalog = catalogsMap[property.CatalogId];
                    if (property.CategoryId != null)
                    {
                        property.Category = result.First(x => x.Id == property.CategoryId);
                    }
                }

                // Fill outlines for products            
                _outlineService.FillOutlinesForObjects(result, catalogId);

                var objectsWithSeo = new List<ISeoSupport>(result);
                var outlineItems = result.Where(c => c.Outlines != null).SelectMany(c => c.Outlines.SelectMany(o => o.Items));
                objectsWithSeo.AddRange(outlineItems);
                _commerceService.LoadSeoForObjects(objectsWithSeo.ToArray());

                return result.ToArray();
            });
        }
    }
}
