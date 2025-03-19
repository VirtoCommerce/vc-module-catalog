using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CoreModule.Core.Outlines;
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

    private const string SeoCategory = "Category";
    private const string SeoProduct = "CatalogProduct";

    public CatalogSeoResolver(
        Func<ICatalogRepository> repositoryFactory,
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
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return [];
        }

        var currentEntitySeoInfos = await SearchSeoInfos(segments.Last(), criteria.StoreId, criteria.LanguageCode);

        if (currentEntitySeoInfos.Count == 0)
        {
            return [];

            // TODO: Uncomment this block of code when the frontend starts to support inactive SEO entries and redirect to the actual ones
            // Try to find inactive SEO entries
            //currentEntitySeoInfos = await SearchSeoInfos(segments.Last(), criteria.StoreId, criteria.LanguageCode, isActive: false);
            //if (currentEntitySeoInfos.Count == 0)
            //{
            //    return [];
            //}
        }

        var groups = currentEntitySeoInfos.GroupBy(x => new { x.ObjectType, x.ObjectId }).ToList();

        if (groups.Count == 1)
        {
            return [currentEntitySeoInfos.First()];
        }

        var parentIds = new List<string>();
        var store = await _storeService.GetByIdAsync(criteria.StoreId);

        // It's not possible to resolve because we don't have parent segment
        if (segments.Length == 1)
        {
            parentIds.Add(store.Catalog);
        }
        else
        {
            // We found multiple SEO records, need to find the correct one by checking the parents.
            var parentSearchCriteria = criteria.CloneTyped();
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
            var objectType = groupKey.ObjectType;
            var objectId = groupKey.ObjectId;

            var outlines = await GetOutlines(objectType, objectId);

            if (LongestOutlineContainsAnyParentId(outlines, store.Catalog, parentIds))
            {
                return currentEntitySeoInfos
                    .Where(x =>
                        x.ObjectId.EqualsIgnoreCase(objectId) &&
                        x.ObjectType.EqualsIgnoreCase(objectType))
                    .ToList();
            }
        }

        return [];
    }

    private async Task<IList<Outline>> GetOutlines(string objectType, string objectId)
    {
        var outlines = objectType switch
        {
            SeoCategory => (await _categoryService.GetByIdAsync(objectId, CategoryResponseGroup.WithOutlines.ToString(), clone: false))?.Outlines,
            SeoProduct => (await _itemService.GetByIdAsync(objectId, ItemResponseGroup.WithOutlines.ToString(), clone: false))?.Outlines,
            _ => [],
        };

        if (outlines is null)
        {
            throw new InvalidOperationException($"{objectType} with ID '{objectId}' was not found.");
        }

        return outlines;
    }

    private static bool LongestOutlineContainsAnyParentId(IList<Outline> outlines, string catalogId, IList<string> parentIds)
    {
        if (outlines.Count == 0)
        {
            return false;
        }

        // Find the length of the longest outline for the given catalog
        var maxLength = outlines
            .Where(x => x.Items.ContainsCatalog(catalogId))
            .Select(x => x.Items.Count)
            .DefaultIfEmpty(0)
            .Max();

        // The last element is the current object.
        // Get the second last element of each longest path.
        var immediateParentIds = outlines
            .Where(x => x.Items.Count == maxLength)
            .SelectMany(o => o.Items.Skip(o.Items.Count - 2).Take(1).Select(i => i.Id))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return immediateParentIds.Any(x => parentIds.Contains(x, StringComparer.OrdinalIgnoreCase));
    }

    private async Task<List<SeoInfo>> SearchSeoInfos(string slug, string storeId, string languageCode, bool isActive = true)
    {
        using var repository = _repositoryFactory();

        var entities = await repository.SeoInfos
            .Where(x =>
                x.IsActive == isActive &&
                x.Keyword == slug &&
                (string.IsNullOrEmpty(x.StoreId) || x.StoreId == storeId) &&
                (string.IsNullOrEmpty(x.Language) || x.Language == languageCode))
            .ToListAsync();

        return entities
            .Select(x => x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance()))
            .OrderByDescending(GetScore)
            .ToList();

        int GetScore(SeoInfo seoInfo)
        {
            var score = 0;
            var hasStoreCriteria = !string.IsNullOrEmpty(storeId);
            var hasLangCriteria = !string.IsNullOrEmpty(languageCode);

            if (hasStoreCriteria && seoInfo.StoreId.EqualsIgnoreCase(storeId))
            {
                score += 2;
            }

            if (hasLangCriteria && seoInfo.LanguageCode.EqualsIgnoreCase(languageCode))
            {
                score += 1;
            }

            return score;
        }
    }
}
