using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.Seo.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class BrandSeoResolver : ISeoResolver
{
    private readonly IBrandSettingService _brandSettingService;
    private readonly ICategoryService _categoryService;
    private readonly ICatalogService _catalogService;
    private readonly Func<ICatalogRepository> _repositoryFactory;
    private readonly CatalogSeoResolver _catalogSeoResolver;

    private const string BrandSeoType = "Brand";
    private const string BrandsSeoType = "Brands";

    public BrandSeoResolver(
        IBrandSettingService brandSettingService,
        ICategoryService categoryService,
        ICatalogService catalogService,
        Func<ICatalogRepository> repositoryFactory,
        CatalogSeoResolver catalogSeoResolver)
    {
        _brandSettingService = brandSettingService;
        _categoryService = categoryService;
        _catalogService = catalogService;
        _repositoryFactory = repositoryFactory;
        _catalogSeoResolver = catalogSeoResolver;
    }

    public async Task<IList<SeoInfo>> FindSeoAsync(SeoSearchCriteria criteria)
    {
        var brandStoreSettings = await _brandSettingService.GetByStoreIdAsync(criteria.StoreId);
        if (!BrandsCatalogEnabled(brandStoreSettings))
        {
            return [];
        }

        var permalink = criteria.Permalink ?? string.Empty;
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return [];
        }

        if (segments.Length == 1)
        {
            // possible brands root page seo
            var brandsSeoInfo = await FindBrandsCatalogSeoInfo(brandStoreSettings, criteria);
            if (brandsSeoInfo != null)
            {
                return [brandsSeoInfo];
            }
        }

        if (segments.Length == 2)
        {
            // possible brands second level page seo
            var criteriaCopy = criteria.Clone() as SeoSearchCriteria;
            criteriaCopy.Permalink = segments.First();
            var brandsSeoInfo = await FindBrandsCatalogSeoInfo(brandStoreSettings, criteriaCopy);
            if (brandsSeoInfo == null)
            {
                return [];
            }

            // brands catalog seo found meaning this is seo request for brands page, find second level (actual brand)
            var brandsCatalog = await _catalogService.GetNoCloneAsync(brandStoreSettings.BrandCatalogId);
            var brandCategory = await GetCategorySeo(criteria, segments, brandsCatalog);

            if (brandCategory != null)
            {
                var brandSeoInfo = CreateSeoInfo(BrandSeoType, criteria.Slug, criteria);
                brandSeoInfo.Id = brandCategory.Id;
                brandSeoInfo.ObjectId = brandCategory.Id;
                return [brandSeoInfo];
            }
        }

        return [];
    }

    private static bool BrandsCatalogEnabled(BrandStoreSetting brandStoreSettings)
    {
        return brandStoreSettings != null && brandStoreSettings.BrandsEnabled && brandStoreSettings.BrandCatalogId != null;
    }

    private async Task<SeoInfo> FindBrandsCatalogSeoInfo(BrandStoreSetting brandStoreSettings, SeoSearchCriteria criteria)
    {
        var catalogSeoInfos = await _catalogSeoResolver.FindSeoAsync(criteria);
        var brandsCatalogSeoInfo = catalogSeoInfos.FirstOrDefault(x => x.ObjectType == "Catalog" && x.ObjectId == brandStoreSettings.BrandCatalogId);
        if (brandsCatalogSeoInfo != null)
        {
            var brandsSeoInfo = CreateSeoInfo(BrandsSeoType, BrandsSeoType, criteria, brandsCatalogSeoInfo);
            return brandsSeoInfo;
        }

        return null;
    }

    private static SeoInfo CreateSeoInfo(string seoType, string id, SeoSearchCriteria criteria, SeoInfo sourceSeoInfo = null)
    {
        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
        seoInfo.ObjectType = seoType;
        seoInfo.Id = id;
        seoInfo.ObjectId = id;
        seoInfo.StoreId = criteria.StoreId;
        seoInfo.LanguageCode = criteria.LanguageCode;
        seoInfo.SemanticUrl = criteria.Permalink;

        if (sourceSeoInfo != null)
        {
            seoInfo.PageTitle = sourceSeoInfo.PageTitle;
            seoInfo.MetaDescription = sourceSeoInfo.MetaDescription;
            seoInfo.MetaKeywords = sourceSeoInfo.MetaKeywords;
            seoInfo.ImageAltDescription = sourceSeoInfo.ImageAltDescription;
        }

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
}
