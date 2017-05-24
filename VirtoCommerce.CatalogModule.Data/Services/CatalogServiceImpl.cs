using System;
using System.Collections.Generic;
using System.Linq;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
using coreModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogServiceImpl : CatalogServiceBase, ICatalogService
    {
        private readonly ICommerceService _commerceService;
        public CatalogServiceImpl(Func<ICatalogRepository> catalogRepositoryFactory, ICommerceService commerceService, ICacheManager<object> cacheManager)
            : base(catalogRepositoryFactory, cacheManager)
        {
            _commerceService = commerceService;
        }

        #region ICatalogService Members

        public coreModel.Catalog GetById(string catalogId)
        {
            coreModel.Catalog retVal = null;
            var dbCatalog =  base.AllCachedCatalogs.FirstOrDefault(x => x.Id == catalogId);
            if(dbCatalog != null)
            {
                retVal = dbCatalog.ToCoreModel();
            }
            return retVal;
        }

        public coreModel.Catalog Create(coreModel.Catalog catalog)
        {
            SaveChanges(new[] { catalog });
            var result = GetById(catalog.Id);
            return result;
        }

        public void Update(coreModel.Catalog[] catalogs)
        {
            SaveChanges(catalogs);
        }

        public void Delete(string[] catalogIds)
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                repository.RemoveCatalogs(catalogIds);
                CommitChanges(repository);
                //Reset cached categories and catalogs
                base.InvalidateCache();
            }
        }


        public IEnumerable<coreModel.Catalog> GetCatalogsList()
        {
            return base.AllCachedCatalogs.Select(x => x.ToCoreModel()).OrderBy(x => x.Name);
        }

        #endregion

        protected virtual void SaveChanges(coreModel.Catalog[] catalogs)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = base.CatalogRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var dbExistEntities = repository.GetCatalogsByIds(catalogs.Where(x => !x.IsTransient()).Select(x => x.Id).ToArray());
                foreach (var catalog in catalogs)
                {
                    var originalEntity = dbExistEntities.FirstOrDefault(x => x.Id == catalog.Id);
                    var modifiedEntity = catalog.ToDataModel(pkMap);
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
