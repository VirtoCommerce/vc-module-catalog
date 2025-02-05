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

public class CatalogSeoResolver : ISeoResolver
{
    private readonly Func<ICatalogRepository> _repositoryFactory;
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IStoreService _storeService;

    private const string CategoryObjectType = "Category";
    private const string CatalogProductObjectType = "CatalogProduct";

    public CatalogSeoResolver(Func<ICatalogRepository> repositoryFactory,
        ICategoryService categoryService,
        IItemService itemService,
        IStoreService storeService)
    {
        _repositoryFactory = repositoryFactory;
        _categoryService = categoryService;
        _itemService = itemService;
        _storeService = storeService;
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
            return [];

            // TODO: Uncomment this block of code when frontend will support deactivated seo entries and redirect it to real seo 
            // Try to find deactivated seo entries and revert it back if we found it
            //currentEntitySeoInfos = await SearchSeoInfos(criteria.StoreId, criteria.LanguageCode, segments.Last(), false);
            //if (currentEntitySeoInfos.Count == 0)
            //{
            //    return [];
            //}
        }

        var groups = currentEntitySeoInfos.GroupBy(x => new { x.ObjectType, x.ObjectId });

        // We found seo information by seo search criteria
        if (groups.Count() == 1)
        {
            return [currentEntitySeoInfos.First()];
        }

        var parentIds = new List<string>();

        var store = await _storeService.GetByIdAsync(criteria.StoreId);

        // It's not possibe to resolve because we don't have parent segment
        if (segments.Length == 1)
        {
            parentIds.Add(store.Catalog);
        }
        else
        {
            // We found multiple seo information by seo search criteria, need to find correct by checking parent.
            var parentSearchCriteria = criteria.Clone() as SeoSearchCriteria;
            parentSearchCriteria.Permalink = string.Join('/', segments.Take(segments.Length - 1));
            var parentSeoInfos = await FindSeoAsync(parentSearchCriteria);

            if (parentSeoInfos.Count == 0)
            {
                return [];
            }

            parentIds.AddRange(parentSeoInfos.Select(x => x.ObjectId).Distinct());
        }

        foreach (var groupKey in groups.Select(g => g.Key))
        {
            if (groupKey.ObjectType.Equals(CategoryObjectType, StringComparison.OrdinalIgnoreCase))
            {
                var isMatch = await DoesParentMatchCategoryOutline(store.Catalog, parentIds, groupKey.ObjectId);
                if (isMatch)
                {
                    return currentEntitySeoInfos.Where(x =>
                        groupKey.ObjectId.Equals(x.ObjectId, StringComparison.OrdinalIgnoreCase)
                        && groupKey.ObjectType.Equals(CategoryObjectType, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            // Inside the method
            else if (groupKey.ObjectType.Equals(CatalogProductObjectType, StringComparison.OrdinalIgnoreCase))
            {
                var isMatch = await DoesParentMatchProductOutline(store.Catalog, parentIds, groupKey.ObjectId);

                if (isMatch)
                {
                    return currentEntitySeoInfos.Where(x =>
                        groupKey.ObjectId.Equals(x.ObjectId, StringComparison.OrdinalIgnoreCase)
                        && groupKey.ObjectType.Equals(CatalogProductObjectType, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
        }

        return [];
    }

    private async Task<bool> DoesParentMatchCategoryOutline(string catalogId, IList<string> parentCategorieIds, string objectId)
    {
        var category = await _categoryService.GetByIdAsync(objectId, CategoryResponseGroup.WithOutlines.ToString(), false);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID '{objectId}' was not found.");
        }

        if (category.Outlines.Count == 0)
        {
            return false;
        }

        // Select outline for current catalog and longest path to find real parent
        var maxLength = category.Outlines
            .Where(x => x.Items.First().Id.Equals(catalogId))
            .Select(x => x.Items.Count)
            .DefaultIfEmpty(0)
            .Max();

        // Get parent from longest path. Keep in mind that latest element is current object id.
        var categoryParents = category.Outlines
            .Where(x => x.Items.Count == maxLength)
            .SelectMany(x => x.Items.Skip(x.Items.Count - 2).Take(1).Select(i => i.Id))
            .Distinct()
            .ToList();

        return categoryParents.Any(parentCategorieIds.Contains);
    }

    private async Task<bool> DoesParentMatchProductOutline(string catalogId, IList<string> parentCategorieIds, string objectId)
    {
        var product = await _itemService.GetByIdAsync(objectId, CategoryResponseGroup.WithOutlines.ToString(), false);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{objectId}' was not found.");
        }

        // Select outline for current catalog and longest path to find real parent
        var maxLength = product.Outlines
            .Where(x => x.Items.First().Id.Equals(catalogId))
            .Select(x => x.Items.Count)
            .Max();

        // Get parent from longest path. Keep in mind that latest element is current product id.
        var categoryParents = product.Outlines
            .Where(x => x.Items.Count == maxLength)
            .SelectMany(x => x.Items.Select(x => x.Id)
            .Skip(x.Items.Count - 2)
            .Take(1))
            .Distinct()
            .ToList();

        return categoryParents.Any(parentCategorieIds.Contains);
    }

    private async Task<List<SeoInfo>> SearchSeoInfos(string storeId, string languageCode, string slug, bool isActive = true)
    {
        using var repository = _repositoryFactory();

        return (await repository.SeoInfos.Where(s => s.IsActive == isActive
            && s.Keyword == slug
            && (string.IsNullOrEmpty(s.StoreId) || s.StoreId == storeId)
            && (string.IsNullOrEmpty(s.Language) || s.Language == languageCode))
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

        if (hasStoreCriteria && string.Equals(seoInfo.StoreId, storeId, StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        if (hasLangCriteria && string.Equals(seoInfo.LanguageCode, language, StringComparison.OrdinalIgnoreCase))
        {
            score += 1;
        }

        return score;
    }
}

