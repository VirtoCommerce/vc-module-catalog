using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule2.Data.Services
{
    public class CatalogService2 : CatalogService
    {
        public CatalogService2(Func<ICatalogRepository> catalogRepositoryFactory,
                          IEventPublisher eventPublisher, IPlatformMemoryCache platformMemoryCache, AbstractValidator<IHasProperties> hasPropertyValidator) :
            base(catalogRepositoryFactory, eventPublisher, platformMemoryCache, hasPropertyValidator)
        {
        }
        public override Task DeleteAsync(string[] catalogIds)
        {
            return base.DeleteAsync(catalogIds);
        }
        public override Task<Catalog[]> GetByIdsAsync(string[] catalogIds, string responseGroup = null)
        {
            return base.GetByIdsAsync(catalogIds, responseGroup);
        }
        protected override void LoadDependencies(IEnumerable<Catalog> catalogs, IDictionary<string, Catalog> preloadedCatalogsMap)
        {
            base.LoadDependencies(catalogs, preloadedCatalogsMap);
        }
        protected override Task<IDictionary<string, Catalog>> PreloadCatalogsAsync()
        {
            return base.PreloadCatalogsAsync();
        }
        public override Task SaveChangesAsync(Catalog[] catalogs)
        {
            return base.SaveChangesAsync(catalogs);
        }
    }
}
