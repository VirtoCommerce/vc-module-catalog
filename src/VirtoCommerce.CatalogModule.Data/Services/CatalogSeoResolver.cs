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

namespace VirtoCommerce.CatalogModule.Data.Services;

public class CatalogSeoResolver : ISeoResolver
{
    private readonly Func<ICatalogRepository> _repositoryFactory;
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;

    private const string CategoryObjectType = "Category";
    private const string CatalogProductObjectType = "CatalogProduct";

    public CatalogSeoResolver(Func<ICatalogRepository> repositoryFactory,
        ICategoryService categoryService,
        IItemService itemService)
    {
        _repositoryFactory = repositoryFactory;
        _categoryService = categoryService;
        _itemService = itemService;
    }

    public async Task<IList<SeoInfo>> FindSeoAsync(SeoSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var permalink = criteria.Permalink ?? string.Empty;
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (segments.Length == 0)
        {
            return [];
        }

        var currentEntitySeoInfos = await SearchSeoInfos(criteria.StoreId, criteria.LanguageCode, segments.Last());

        if (currentEntitySeoInfos.Count == 0)
        {
            // Try to find deactivated seo entries and revert it back if we found it
            currentEntitySeoInfos = await SearchSeoInfos(criteria.StoreId, criteria.LanguageCode, segments.Last(), false);
            if (currentEntitySeoInfos.Count == 0)
            {
                return [];
            }
        }

        var groups = currentEntitySeoInfos.GroupBy(x => new { x.ObjectType, x.ObjectId });

        // We found seo information by seo search criteria
        if (groups.Count() == 1)
        {
            return [currentEntitySeoInfos.First()];
        }

        // It's not possibe to resolve because we don't have parent segment
        if (segments.Length == 1)
        {
            return [];
        }

        // We found multiple seo information by seo search criteria, need to find correct by checking parent.
        var parentSearchCriteria = criteria.Clone() as SeoSearchCriteria;
        parentSearchCriteria.Permalink = string.Join('/', segments.Take(segments.Length - 1));
        var parentSeoInfos = await FindSeoAsync(parentSearchCriteria);

        if (parentSeoInfos.Count == 0)
        {
            return [];
        }

        var parentCategorieIds = parentSeoInfos.Select(x => x.ObjectId).Distinct().ToList();

        foreach (var group in groups)
        {
            if (group.Key.ObjectType == CategoryObjectType)
            {
                var isMatch = await DoesParentMatchCategoryOutline(parentCategorieIds, group.Key.ObjectId);
                if (isMatch)
                {
                    return currentEntitySeoInfos.Where(x =>
                        x.ObjectId == group.Key.ObjectId
                        && group.Key.ObjectType == CategoryObjectType).ToList();
                }
            }

            // Inside the method
            else if (group.Key.ObjectType == CatalogProductObjectType)
            {
                var isMatch = await DoesParentMatchProductOutline(parentCategorieIds, group.Key.ObjectId);

                if (isMatch)
                {
                    return currentEntitySeoInfos.Where(x =>
                        x.ObjectId == group.Key.ObjectId
                        && group.Key.ObjectType == CatalogProductObjectType).ToList();
                }
            }
        }

        return [];
    }

    private async Task<bool> DoesParentMatchCategoryOutline(IList<string> parentCategorieIds, string objectId)
    {
        var category = await _categoryService.GetByIdAsync(objectId, CategoryResponseGroup.WithOutlines.ToString(), false);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID '{objectId}' was not found.");
        }
        var outlines = category.Outlines.Select(x => x.Items.Skip(x.Items.Count - 2).First().Id).Distinct().ToList();
        return outlines.Any(parentCategorieIds.Contains);
    }

    private async Task<bool> DoesParentMatchProductOutline(IList<string> parentCategorieIds, string objectId)
    {
        var product = await _itemService.GetByIdAsync(objectId, CategoryResponseGroup.WithOutlines.ToString(), false);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{objectId}' was not found.");
        }
        var outlines = product.Outlines.Select(x => x.Items.Skip(x.Items.Count - 2).First().Id).Distinct().ToList();
        return outlines.Any(parentCategorieIds.Contains);
    }

    private async Task<List<SeoInfo>> SearchSeoInfos(string storeId, string languageCode, string slug, bool isActive = true)
    {
        using var repository = _repositoryFactory();

        return (await repository.SeoInfos.Where(s => s.IsActive == isActive
            && s.Keyword == slug
            && (s.StoreId == null || s.StoreId == storeId)
            && (s.Language == null || s.Language == languageCode))
            .ToListAsync())
            .Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance()))
            .OrderByDescending(s => GetPriorityScore(s, storeId, languageCode))
            .ToList();
    }

    private static int GetPriorityScore(SeoInfo seoInfo, string storeId, string language)
    {
        var score = 0;
        var hasStoreCriteria = !string.IsNullOrEmpty(storeId);
        var hasLangCriteria = !string.IsNullOrEmpty(language);

        if (hasStoreCriteria && seoInfo.StoreId == storeId)
        {
            score += 2;
        }

        if (hasLangCriteria && seoInfo.LanguageCode == language)
        {
            score += 1;
        }

        return score;
    }
}

