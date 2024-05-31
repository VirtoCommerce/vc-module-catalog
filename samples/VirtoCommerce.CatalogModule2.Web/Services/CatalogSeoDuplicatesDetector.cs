using System;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule2.Web.Services
{
    /// <summary>
    /// Detect SEO duplicates based on store, catalog, categories relationships and structure knowledge
    /// </summary>
    public class CatalogSeoDuplicatesDetector2 : CatalogSeoDuplicatesDetector
    {
        public CatalogSeoDuplicatesDetector2(
            IProductService productService,
            ICategoryService categoryService,
            IStoreService storeService,
            Func<ICatalogRepository> repositoryFactory,
            ISettingsManager settingsManager)
            : base(
                productService,
                categoryService,
                storeService,
                repositoryFactory,
                settingsManager)
        {
        }
    }
}
