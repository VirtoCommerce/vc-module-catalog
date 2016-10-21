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
            var pkMap = new PrimaryKeyResolvingMap();
            var dbCatalog = catalog.ToDataModel(pkMap);
            coreModel.Catalog retVal = null;
            using (var repository = base.CatalogRepositoryFactory())
            {
                repository.Add(dbCatalog);
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }
            retVal = GetById(dbCatalog.Id);
            return retVal;
        }

        public void Update(coreModel.Catalog[] catalogs)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = base.CatalogRepositoryFactory())
            using (var changeTracker = base.GetChangeTracker(repository))
            {
                foreach (var catalog in catalogs)
                {
                    var dbCatalog = repository.GetCatalogsByIds(new[] { catalog.Id }).FirstOrDefault();
                    if (dbCatalog == null)
                    {
                        throw new NullReferenceException("dbCatalog");
                    }
                    var dbCatalogChanged = catalog.ToDataModel(pkMap);

                    changeTracker.Attach(dbCatalog);
                    dbCatalogChanged.Patch(dbCatalog);

                }

                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }
        }

        public void Delete(string[] catalogIds)
        {
            using (var repository = base.CatalogRepositoryFactory())
            {
                repository.RemoveCatalogs(catalogIds);
                CommitChanges(repository);
            }
        }


        public IEnumerable<coreModel.Catalog> GetCatalogsList()
        {
            return base.AllCachedCatalogs.Select(x => x.ToCoreModel()).OrderBy(x => x.Name);
        }

        #endregion
    }
}
