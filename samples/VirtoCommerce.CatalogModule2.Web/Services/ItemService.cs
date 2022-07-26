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
            Func<ICatalogRepository> catalogRepositoryFactory,
            IEventPublisher eventPublisher,
            AbstractValidator<IHasProperties> hasPropertyValidator,
            ICatalogService catalogService,
            ICategoryService categoryService,
            IOutlineService outlineService,
            IPlatformMemoryCache platformMemoryCache,
            IBlobUrlResolver blobUrlResolver,
            ISkuGenerator skuGenerator,
            AbstractValidator<CatalogProduct> productValidator)
            : base(
                catalogRepositoryFactory,
                eventPublisher,
                hasPropertyValidator,
                catalogService,
                categoryService,
                outlineService,
                platformMemoryCache,
                blobUrlResolver,
                skuGenerator,
                productValidator)
        {
        }

        protected override void ApplyInheritanceRules(IEnumerable<CatalogProduct> products)
        {
            base.ApplyInheritanceRules(products);
        }

        protected override void ClearCache(IEnumerable<CatalogProduct> models)
        {
            base.ClearCache(models);
        }

        public override Task DeleteAsync(IEnumerable<string> ids, bool softDelete = false)
        {
            return base.DeleteAsync(ids, softDelete);
        }

        public override Task<CatalogProduct> GetByIdAsync(string itemId, string responseGroup, string catalogId)
        {
            return base.GetByIdAsync(itemId, responseGroup, catalogId);
        }

        public override Task<CatalogProduct[]> GetByIdsAsync(string[] itemIds, string respGroup, string catalogId = null)
        {
            return base.GetByIdsAsync(itemIds, respGroup, catalogId);
        }

        protected override Task LoadDependencies(IList<CatalogProduct> products)
        {
            return base.LoadDependencies(products);
        }

        public override Task SaveChangesAsync(IEnumerable<CatalogProduct> models)
        {
            return base.SaveChangesAsync(models);
        }

        protected override void SetProductDependencies(CatalogProduct product, IDictionary<string, Catalog> catalogsByIdDict, IDictionary<string, Category> categoriesByIdDict)
        {
            base.SetProductDependencies(product, catalogsByIdDict, categoriesByIdDict);
        }

        protected override Task ValidateProductsAsync(CatalogProduct[] products)
        {
            return base.ValidateProductsAsync(products);
        }
    }
}
