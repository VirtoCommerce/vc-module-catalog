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
