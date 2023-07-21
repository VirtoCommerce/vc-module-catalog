using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule2.Web.Services
{
    public class CatalogService2 : CatalogService
    {
        public CatalogService2(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            AbstractValidator<IHasProperties> hasPropertyValidator)
            : base(catalogRepositoryFactory, eventPublisher, platformMemoryCache, hasPropertyValidator)
        {
        }

        public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            return base.DeleteAsync(ids, softDelete);
        }

        public override Task<IList<Catalog>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
        {
            return base.GetAsync(ids, responseGroup, clone);
        }

        protected override void LoadDependencies(IEnumerable<Catalog> catalogs, IDictionary<string, Catalog> preloadedCatalogsMap)
        {
            base.LoadDependencies(catalogs, preloadedCatalogsMap);
        }

        protected override Task<IDictionary<string, Catalog>> PreloadCatalogsAsync()
        {
            return base.PreloadCatalogsAsync();
        }

        public override Task SaveChangesAsync(IList<Catalog> models)
        {
            return base.SaveChangesAsync(models);
        }
    }
}
