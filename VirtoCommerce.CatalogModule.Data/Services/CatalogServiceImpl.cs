using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogServiceImpl : ServiceBase, ICatalogService
    {
        private readonly ICommerceService _commerceService;
        private readonly ICacheManager<object> _cacheManager;
        private readonly Func<ICatalogRepository> _repositoryFactory;

        public CatalogServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICommerceService commerceService, ICacheManager<object> cacheManager)
        {
            _commerceService = commerceService;
            _repositoryFactory = catalogRepositoryFactory;
            _cacheManager = cacheManager;
        }

        #region ICatalogService Members

        public Catalog GetById(string catalogId)
        {
            var result = PreloadCatalogs().Where(x => x.Id == catalogId).FirstOrDefault();
            return result;
        }

        public Catalog Create(Catalog catalog)
        {
            SaveChanges(new[] { catalog });
            var result = GetById(catalog.Id);
            return result;
        }

        public void Update(Catalog[] catalogs)
        {
            SaveChanges(catalogs);
        }

        public void Delete(string[] catalogIds)
        {
            using (var repository = _repositoryFactory())
            {
                repository.RemoveCatalogs(catalogIds);
                CommitChanges(repository);
                //Reset cached categories and catalogs
                ResetCache();
            }
        }


        public IEnumerable<Catalog> GetCatalogsList()
        {
            return PreloadCatalogs().OrderBy(x => x.Name);
        }

        #endregion

        protected virtual void SaveChanges(Catalog[] catalogs)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbExistEntities = repository.GetCatalogsByIds(catalogs.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var catalog in catalogs)
                {
                    var originalEntity = dbExistEntities.FirstOrDefault(x => x.Id == catalog.Id);
                    var modifiedEntity = AbstractTypeFactory<CatalogEntity>.TryCreateInstance().FromModel(catalog, pkMap);
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

        protected virtual Catalog[] PreloadCatalogs()
        {
            return _cacheManager.Get("AllCatalogs", CatalogConstants.CacheRegion, () =>
            {
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
                    return repository.GetCatalogsByIds(repository.Catalogs.Select(x => x.Id).ToArray())
                                      .Select(x => x.ToModel(AbstractTypeFactory<Catalog>.TryCreateInstance()))
                                      .ToArray();
                }
            });
        }
    }
}
