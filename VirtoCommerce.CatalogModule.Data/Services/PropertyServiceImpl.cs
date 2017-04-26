using System;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Services;
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
                var dbProperties = repository.GetAllCatalogProperties(catalogId);
                var result = dbProperties.Select(dbProperty => dbProperty.ToCoreModel(base.AllCachedCatalogs, base.AllCachedCategories)).ToArray();
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

            var dbProperty = property.ToDataModel();
            using (var repository = base.CatalogRepositoryFactory())
            {
                if (property.CategoryId != null)
                {
                    var dbCategory = repository.GetCategoriesByIds(new[] { property.CategoryId }, coreModel.CategoryResponseGroup.Info).FirstOrDefault();
                    if (dbCategory == null)
                    {
                        throw new NullReferenceException("dbCategory");
                    }
                    dbCategory.Properties.Add(dbProperty);
                }
                else
                {
                    var dbCatalog = repository.GetCatalogsByIds(new[] { property.CatalogId }).FirstOrDefault();
                    if (dbCatalog == null)
                    {
                        throw new NullReferenceException("dbCatalog");
                    }
                    dbCatalog.Properties.Add(dbProperty);
                }
                repository.Add(dbProperty);
                CommitChanges(repository);
            }
            var retVal = GetById(dbProperty.Id);
            return retVal;
        }

        public void Update(coreModel.Property[] properties)
        {
            using (var repository = base.CatalogRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbProperties = repository.GetPropertiesByIds(properties.Select(x => x.Id).ToArray());

                foreach (var dbProperty in dbProperties)
                {
                    var property = properties.FirstOrDefault(x => x.Id == dbProperty.Id);
                    if (property != null)
                    {
                        changeTracker.Attach(dbProperty);

                        var dbPropertyChanged = property.ToDataModel();
                        dbPropertyChanged.Patch(dbProperty);
                    }
                }
                CommitChanges(repository);
            }
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
    }
}
