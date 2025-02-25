using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogModule2.Web.Services
{
    public class CategoryService2 : CategoryService
    {
        public CategoryService2(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            AbstractValidator<IHasProperties> hasPropertyValidator,
            ICatalogService catalogService,
            IOutlineService outlineService,
            IBlobUrlResolver blobUrlResolver,
            ISanitizerService sanitizerService)
            : base(
                repositoryFactory,
                platformMemoryCache,
                eventPublisher,
                hasPropertyValidator,
                catalogService,
                outlineService,
                blobUrlResolver,
                sanitizerService)
        {
        }

        public override Task<IList<Category>> GetByIdsAsync(IList<string> ids, string responseGroup, string catalogId)
        {
            return base.GetByIdsAsync(ids, responseGroup, catalogId);
        }

        public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            return base.DeleteAsync(ids, softDelete);
        }

        protected override void ApplyInheritanceRules(ICollection<Category> categories)
        {
            base.ApplyInheritanceRules(categories);
        }

        protected override void ClearCache(IList<Category> models)
        {
            base.ClearCache(models);
        }

        protected override Task LoadDependencies(ICollection<Category> categories, IDictionary<string, Category> preloadedCategoriesMap)
        {
            return base.LoadDependencies(categories, preloadedCategoriesMap);
        }

        protected override void ResolveImageUrls(ICollection<Category> categories)
        {
            base.ResolveImageUrls(categories);
        }

        public override Task SaveChangesAsync(IList<Category> models)
        {
            return base.SaveChangesAsync(models);
        }

        protected override Task ValidateCategoryPropertiesAsync(IList<Category> categories)
        {
            return base.ValidateCategoryPropertiesAsync(categories);
        }
    }
}
