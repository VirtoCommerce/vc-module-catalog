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
    public class ItemService2 : ItemService
    {
        public ItemService2(
            Func<ICatalogRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            AbstractValidator<IHasProperties> hasPropertyValidator,
            ICatalogService catalogService,
            ICategoryService categoryService,
            IOutlineService outlineService,
            IBlobUrlResolver blobUrlResolver,
            ISkuGenerator skuGenerator,
            AbstractValidator<CatalogProduct> productValidator,
            ISanitizerService sanitizerService)
            : base(
                repositoryFactory,
                platformMemoryCache,
                eventPublisher,
                hasPropertyValidator,
                catalogService,
                categoryService,
                outlineService,
                blobUrlResolver,
                skuGenerator,
                productValidator,
                sanitizerService)
        {
        }

        protected override void ApplyInheritanceRules(IList<CatalogProduct> products)
        {
            base.ApplyInheritanceRules(products);
        }

        protected override void ClearCache(IList<CatalogProduct> models)
        {
            base.ClearCache(models);
        }

        public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
        {
            return base.DeleteAsync(ids, softDelete);
        }

        public override Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId)
        {
            return base.GetByIdAsync(itemId, responseGroup, catalogId);
        }

        public override Task<IList<CatalogProduct>> GetByIdsAsync(IList<string> ids, string responseGroup, string catalogId)
        {
            return base.GetByIdsAsync(ids, responseGroup, catalogId);
        }

        protected override Task LoadDependencies(IList<CatalogProduct> products)
        {
            return base.LoadDependencies(products);
        }

        public override Task SaveChangesAsync(IList<CatalogProduct> models)
        {
            return base.SaveChangesAsync(models);
        }

        protected override void SetProductDependencies(CatalogProduct product, IDictionary<string, Catalog> catalogsByIdDict, IDictionary<string, Category> categoriesByIdDict)
        {
            base.SetProductDependencies(product, catalogsByIdDict, categoriesByIdDict);
        }

        protected override Task ValidateProductsAsync(IList<CatalogProduct> products)
        {
            return base.ValidateProductsAsync(products);
        }
    }
}
