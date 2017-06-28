using System;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Extensions;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class PropertyServiceImpl : ServiceBase, IPropertyService
    {
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly ICacheManager<object> _cacheManager;
        public PropertyServiceImpl(Func<ICatalogRepository> repositoryFactory, ICacheManager<object> cacheManager)
        {
            _repositoryFactory = repositoryFactory;
            _cacheManager = cacheManager;
        }

        #region IPropertyService Members

        public Property GetById(string propertyId)
        {
            return GetByIds(new[] { propertyId }).FirstOrDefault();
        }

        public Property[] GetByIds(string[] propertyIds)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var entities = repository.GetPropertiesByIds(propertyIds);
                var result = entities.Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToArray();
                return result;
            }
        }

        public Property[] GetAllCatalogProperties(string catalogId)
        {
            using (var repository = _repositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var entities = repository.GetAllCatalogProperties(catalogId);
                var result = entities.Select(x => x.ToModel(AbstractTypeFactory<Property>.TryCreateInstance())).ToArray();
                return result;
            }
        }


        public Property[] GetAllProperties()
        {
            using (var repository = _repositoryFactory())
            {
                return GetByIds(repository.Properties.Select(x => x.Id).ToArray());
            }
        }


        public Property Create(Property property)
        {
            if (property.CatalogId == null)
            {
                throw new NullReferenceException("property.CatalogId");
            }

            SaveChanges(new[] { property });

            var result = GetById(property.Id);
            return result;
        }

        public void Update(Property[] properties)
        {
            SaveChanges(properties);
        }


        public void Delete(string[] propertyIds)
        {
            using (var repository = _repositoryFactory())
            {
                var entities = repository.GetPropertiesByIds(propertyIds);

                foreach (var entity in entities)
                {
                    repository.Remove(entity);
                }

                CommitChanges(repository);
                //Reset cached categories and catalogs
                ResetCache();
            }
        }

        public PropertyDictionaryValue[] SearchDictionaryValues(string propertyId, string keyword)
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

        protected virtual void SaveChanges(Property[] properties)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbExistEntities = repository.GetPropertiesByIds(properties.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var property in properties)
                {
                    var originalEntity = dbExistEntities.FirstOrDefault(x => x.Id == property.Id);
                    var modifiedEntity = AbstractTypeFactory<PropertyEntity>.TryCreateInstance().FromModel(property, pkMap);
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
                ResetCache();
            }
        }

        protected virtual void ResetCache()
        {
            _cacheManager.ClearRegion(CatalogConstants.CacheRegion);
        }
    }
}
