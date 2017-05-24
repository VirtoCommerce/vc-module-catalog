using System;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using coreModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyServiceImpl : CatalogServiceBase, IPropertyService
    {
        public PropertyServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICacheManager<object> cacheManager)
            : base(catalogRepositoryFactory, cacheManager)
        {
        }

        #region IPropertyService Members

        public coreModel.Property GetById(string propertyId)
        {
            return GetByIds(new[] { propertyId }).FirstOrDefault();
        }

        public coreModel.Property[] GetByIds(string[] propertyIds)
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                var dbProperties = repository.GetPropertiesByIds(propertyIds);
                var result = dbProperties.Select(dbProperty => dbProperty.ToCoreModel(base.AllCachedCatalogs, base.AllCachedCategories)).ToArray();
                return result;
            }
        }

        public coreModel.Property[] GetAllCatalogProperties(string catalogId)
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                var allCachedCatalogs = base.AllCachedCatalogs;
                var allCachedCategories = base.AllCachedCategories;
                var dbProperties = repository.GetAllCatalogProperties(catalogId);
                var result = dbProperties.Select(dbProperty => dbProperty.ToCoreModel(allCachedCatalogs, allCachedCategories)).ToArray();
                return result;
            }
        }


        public coreModel.Property[] GetAllProperties()
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                return GetByIds(repository.Properties.Select(x => x.Id).ToArray());
            }
        }


        public coreModel.Property Create(coreModel.Property property)
        {
            if (property.CatalogId == null)
            {
                throw new NullReferenceException("property.CatalogId");
            }

            SaveChanges(new[] { property });

            var result = GetById(property.Id);
            return result;
        }

        public void Update(coreModel.Property[] properties)
        {
            SaveChanges(properties);
        }


        public void Delete(string[] propertyIds)
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                var dbProperties = repository.GetPropertiesByIds(propertyIds);

                foreach (var dbProperty in dbProperties)
                {
                    repository.Remove(dbProperty);
                }

                CommitChanges(repository);
                //Reset cached categories and catalogs
                base.InvalidateCache();
            }
        }

        public coreModel.PropertyDictionaryValue[] SearchDictionaryValues(string propertyId, string keyword)
        {
            var property = GetById(propertyId);
            var result = property.DictionaryValues.ToArray();
            if (!String.IsNullOrEmpty(keyword))
            {
                result = result.Where(x => x.Value.Contains(keyword)).ToArray();
            }
            return result;         
        }
        #endregion

        protected virtual void SaveChanges(coreModel.Property[] properties)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = base.CatalogRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbExistEntities = repository.GetPropertiesByIds(properties.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var property in properties)
                {
                    var originalEntity = dbExistEntities.FirstOrDefault(x => x.Id == property.Id);
                    var modifiedEntity = property.ToDataModel(pkMap);
                    if (originalEntity != null)
                    {
                        changeTracker.Attach(originalEntity);
                        modifiedEntity.Patch(originalEntity);
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
                //Reset cached categories and catalogs
                base.InvalidateCache();
            }
        }
    }
}
