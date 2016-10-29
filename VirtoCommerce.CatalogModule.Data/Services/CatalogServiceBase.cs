using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheManager.Core;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public class CatalogServiceBase : ServiceBase
    {
       
        private readonly ICacheManager<object> _cacheManager;
        private static object _lock = new object();
        public CatalogServiceBase(Func<ICatalogRepository> catalogRepositoryFactory, ICacheManager<object> cacheManager)
        {
            CatalogRepositoryFactory = catalogRepositoryFactory;
            _cacheManager = cacheManager;

        }

        protected override void CommitChanges(IRepository repository)
        {
            _cacheManager.ClearRegion("CatalogModuleRegion");
            base.CommitChanges(repository);
        }

        protected virtual Catalog[] AllCachedCatalogs
        {
            get
            {
                return _cacheManager.Get("AllCatalogs", "CatalogModuleRegion", () =>
                {
                    lock (_lock)
                    {
                        using (var repository = CatalogRepositoryFactory())
                        {
                            //EF multi-thread issue for cached entities
                            //http://stackoverflow.com/questions/29106477/nullreferenceexception-in-entity-framework-from-trygetcachedrelatedend
                            var dbConfiguration = ((System.Data.Entity.DbContext)repository).Configuration;
                            dbConfiguration.ProxyCreationEnabled = false;
                            dbConfiguration.AutoDetectChangesEnabled = false;
                            return repository.GetCatalogsByIds(repository.Catalogs.Select(x => x.Id).ToArray());
                        }
                    }
                });            

            }
        }

        protected virtual Category[] AllCachedCategories
        {
            get
            {
                return _cacheManager.Get("AllCategories", "CatalogModuleRegion", () =>
                {
                    lock (_lock)
                    {
                        using (var repository = CatalogRepositoryFactory())
                        {
                            //EF multi-thread issue for cached entities
                            //http://stackoverflow.com/questions/29106477/nullreferenceexception-in-entity-framework-from-trygetcachedrelatedend
                            var dbConfiguration = ((System.Data.Entity.DbContext)repository).Configuration;
                            dbConfiguration.ProxyCreationEnabled = false;
                            dbConfiguration.AutoDetectChangesEnabled = false;
                            return repository.GetCategoriesByIds(repository.Categories.Select(x => x.Id).ToArray(), Domain.Catalog.Model.CategoryResponseGroup.Full);
                        }
                    }
                });

            }
        }

        protected Func<ICatalogRepository> CatalogRepositoryFactory
        {
            get;
            private set;
        }
    }
}
