using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    /// <summary>
    /// Detect SEO duplicates based on store, catalog, categories relationships and structure knowledge
    /// </summary>
    public class CatalogSeoDuplicatesDetector : ISeoDuplicatesDetector
    {
        private readonly IItemService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IStoreService _storeService;
        private readonly Func<ICatalogRepository> _repositoryFactory;
        private readonly ISettingsManager _settingsManager;

        public CatalogSeoDuplicatesDetector(
            IItemService productService,
            ICategoryService categoryService,
            IStoreService storeService,
            Func<ICatalogRepository> repositoryFactory,
            ISettingsManager settingsManager)
        {
            _productService = productService;
            _categoryService = categoryService;
            _storeService = storeService;
            _repositoryFactory = repositoryFactory;
            _settingsManager = settingsManager;
        }

        #region ISeoConflictDetector Members

        public async Task<IEnumerable<SeoInfo>> DetectSeoDuplicatesAsync(TenantIdentity tenantIdentity)
        {
            var useSeoDeduplication = await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.UseSeoDeduplication);
            var retVal = new List<SeoInfo>();

            if (useSeoDeduplication)
            {
                string catalogId = null;
                var objectType = tenantIdentity.Type;
                var objectId = tenantIdentity.Id;

                if (objectType.EqualsInvariant(nameof(Store)))
                {
                    var store = await _storeService.GetNoCloneAsync(tenantIdentity.Id);
                    if (store != null)
                    {
                        catalogId = store.Catalog;
                    }
                }
                else if (objectType.EqualsInvariant(nameof(Category)))
                {
                    var category = await _categoryService.GetNoCloneAsync(objectId, CategoryResponseGroup.Info.ToString());
                    if (category != null)
                    {
                        catalogId = category.CatalogId;
                    }
                }
                else if (objectType.EqualsInvariant(nameof(CatalogProduct)))
                {
                    var product = await _productService.GetNoCloneAsync(objectId, ItemResponseGroup.ItemInfo.ToString());
                    if (product != null)
                    {
                        catalogId = product.CatalogId;
                    }
                }

                if (!string.IsNullOrEmpty(catalogId))
                {
                    //Get all SEO owners witch related to requested catalog and contains Seo duplicates
                    var objectsWithSeoDuplicates = await GetSeoOwnersContainsDuplicatesAsync(catalogId);
                    //Need select for each seo owner one seo defined for requested container or without it if not exist
                    retVal = objectsWithSeoDuplicates
                        .Select(x =>
                            x.SeoInfos
                                .Where(s => s.StoreId == objectId || string.IsNullOrEmpty(s.StoreId))
                                .OrderByDescending(s => s.StoreId)
                                .FirstOrDefault())
                        .Where(x => x != null)
                        //return only Seo infos that have duplicate slug keyword
                        .GroupBy(x => x.SemanticUrl)
                        .Where(x => x.Count() > 1)
                        .SelectMany(x => x)
                        .ToList();
                }
            }

            return retVal;
        }

        #endregion ISeoConflictDetector Members

        /// <summary>
        /// Detect SEO duplicates for object belongs to catalog  (physical or virtual) based on links information
        /// </summary>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        private async Task<IList<ISeoSupport>> GetSeoOwnersContainsDuplicatesAsync(string catalogId)
        {
            var allDuplicates = await GetAllSeoDuplicatesAsync();

            var productIds = allDuplicates
                .Where(x => x.ObjectType.EqualsInvariant(nameof(CatalogProduct)))
                .Select(x => x.ObjectId)
                .Distinct()
                .ToList();

            var categoryIds = allDuplicates
                .Where(x => x.ObjectType.EqualsInvariant(nameof(Category)))
                .Select(x => x.ObjectId)
                .Distinct()
                .ToList();

            var products = await _productService.GetByIdsAsync(productIds, (ItemResponseGroup.Outlines | ItemResponseGroup.Seo).ToString(), catalogId);
            var categories = await _categoryService.GetByIdsAsync(categoryIds, (CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithSeo).ToString(), catalogId);

            var retVal = new List<ISeoSupport>();
            //Here we try to find between SEO duplicates records for products with directly or indirectly (virtual) related to requested catalog
            foreach (var product in products)
            {
                if (product.CatalogId == catalogId || product.Outlines.SelectMany(x => x.Items).Any(x => x.Id == catalogId))
                {
                    foreach (var productSeo in product.SeoInfos)
                    {
                        productSeo.Name = $"{product.Name} ({product.Code})";
                    }
                    retVal.Add(product);
                }
            }
            //Here we try to find between SEO duplicates records for categories with directly or indirectly related to requested catalog
            foreach (var category in categories)
            {
                if (category.CatalogId == catalogId || category.Outlines.SelectMany(x => x.Items).Any(x => x.Id == catalogId))
                {
                    foreach (var categorySeo in category.SeoInfos)
                    {
                        categorySeo.Name = $"{category.Name}";
                    }
                    retVal.Add(category);
                }
            }
            return retVal;
        }

        private async Task<IList<SeoInfo>> GetAllSeoDuplicatesAsync()
        {
            using var repository = _repositoryFactory();
            var duplicateIds = await repository.GetAllSeoDuplicatesIdsAsync();

            var duplicateSeoRecords = await repository.SeoInfos
                .Where(x => duplicateIds.Contains(x.Id))
                .ToListAsync();

            return duplicateSeoRecords
                .Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance()))
                .ToList();
        }
    }
}
