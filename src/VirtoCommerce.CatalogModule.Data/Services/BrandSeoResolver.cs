using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;
public class BrandSeoResolver : ISeoResolver
{
    private readonly IBrandSettingService _brandSettingService;
    private readonly ICategoryService _categoryService;
    private readonly ICatalogService _catalogService;
    private readonly IStoreService _storeService;
    private readonly Func<ICatalogRepository> _repositoryFactory;

    private const string BrandSeoType = "Brand";
    private const string BrandsSeoType = "Brands";

    public BrandSeoResolver(
        IBrandSettingService brandSettingService,
        ICategoryService categoryService,
        ICatalogService catalogService,
        IStoreService storeService,
        Func<ICatalogRepository> repositoryFactory)
    {
        _brandSettingService = brandSettingService;
        _categoryService = categoryService;
        _catalogService = catalogService;
        _storeService = storeService;
        _repositoryFactory = repositoryFactory;
    }

    public async Task<IList<SeoInfo>> FindSeoAsync(SeoSearchCriteria criteria)
    {
        var brandStoreSettings = await _brandSettingService.GetByStoreIdAsync(criteria.StoreId);
        if (brandStoreSettings == null || !brandStoreSettings.BrandsEnabled)
        {
            return [];
        }

        var permalink = criteria.Permalink ?? string.Empty;
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return [];
        }

        if (permalink.EqualsIgnoreCase(BrandsSeoType))
        {
            var seoInfo = CreateSeoInfo(BrandsSeoType, BrandsSeoType, criteria);
            return [seoInfo];
        }

        if (brandStoreSettings.BrandCatalogId != null)
        {
            var catalog = await _catalogService.GetNoCloneAsync(brandStoreSettings.BrandCatalogId);
            if (!IsBrandCatalogQuery(catalog, segments))
            {
                return [];
            }

            var seoInfo = CreateSeoInfo(BrandSeoType, criteria.Slug, criteria);

            var brandCategory = await GetCategorySeo(criteria, segments, catalog);
            if (brandCategory != null)
            {
                seoInfo.Id = brandCategory.Id;
                seoInfo.ObjectId = brandCategory.Id;
            }

            return [seoInfo];
        }

        return [];
    }

    private static SeoInfo CreateSeoInfo(string seoType, string id, SeoSearchCriteria criteria)
    {
        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
        seoInfo.ObjectType = seoType;
        seoInfo.Id = id;
        seoInfo.ObjectId = id;
        seoInfo.StoreId = criteria.StoreId;
        seoInfo.SemanticUrl = criteria.Permalink;
        seoInfo.LanguageCode = criteria.LanguageCode;

        return seoInfo;
    }

    private async Task<Category> GetCategorySeo(SeoSearchCriteria criteria, string[] segments, Catalog catalog)
    {
        var categorySeos = await SearchSeoInfos(segments.Last(), criteria.StoreId, criteria.LanguageCode);
        var categories = await _categoryService.GetNoCloneAsync(categorySeos.Select(x => x.ObjectId).ToArray());
        var brandCategory = categories.FirstOrDefault(x => x.CatalogId == catalog.Id);
        return brandCategory;
    }

    private async Task<List<SeoInfo>> SearchSeoInfos(string slug, string storeId, string languageCode, bool isActive = true)
    {
        using var repository = _repositoryFactory();

        var entities = await repository.SeoInfos
            .Where(x =>
                x.IsActive == isActive &&
                x.Keyword == slug &&
                x.CategoryId != null &&
                (string.IsNullOrEmpty(x.StoreId) || x.StoreId == storeId) &&
                (string.IsNullOrEmpty(x.Language) || x.Language == languageCode))
            .ToListAsync();

        return entities
            .Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance()))
            .ToList();
    }

    private static bool IsBrandCatalogQuery(Catalog catalog, string[] segments)
    {
        return catalog != null && segments.First().EqualsIgnoreCase(catalog.Name);
    }
}
