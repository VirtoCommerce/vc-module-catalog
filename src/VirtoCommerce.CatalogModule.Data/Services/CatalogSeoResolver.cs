using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.Seo.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class CatalogSeoResolver : ISeoResolver
{
    private readonly Func<ICatalogRepository> _repositoryFactory;
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IStoreService _storeService;

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

    public virtual async Task<IList<SeoInfo>> FindSeoAsync(SeoSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var permalink = criteria.Permalink ?? string.Empty;
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return [];
        }

        var store = await _storeService.GetByIdAsync(criteria.StoreId);

        if (store == null)
        {
            return [];
        }

        var currentEntitySeoInfos = await SearchSeoInfos(permalink, store, criteria.LanguageCode);

        if (currentEntitySeoInfos.Count == 0)
        {
            return [];
        }

        var groups = currentEntitySeoInfos.GroupBy(x => new { x.ObjectType, x.ObjectId }).ToList();

        if (groups.Count == 1)
        {
            return [currentEntitySeoInfos.First()];
        }

        var parentIds = new List<string>();

        // It's not possible to resolve because we don't have parent segment
        if (segments.Length == 1)
        {
            parentIds.Add(store.Catalog);
        }
        else
        {
            // We found multiple SEO records, need to choose the correct one by checking the parents recursively.
            var parentSearchCriteria = criteria.CloneTyped();
            parentSearchCriteria.Permalink = string.Join('/', segments.Take(segments.Length - 1));
            var parentSeoInfos = await FindSeoAsync(parentSearchCriteria);

            if (parentSeoInfos.Count == 0)
            {
                return [];
            }

            parentIds.AddRange(parentSeoInfos.Select(x => x.ObjectId).Distinct());
        }

        foreach (var group in groups)
        {
            var objectType = group.Key.ObjectType;
            var objectId = group.Key.ObjectId;

            var outlines = await GetOutlines(objectType, objectId, group.ToList());

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


    private async Task<IList<Outline>> GetOutlines(string objectType, string objectId, IList<SeoInfo> infos)
    {
        var outlines = objectType switch
        {
            SeoExtensions.SeoCatalog => CreateCatalogOutline(objectId, infos),
            SeoExtensions.SeoCategory => (await _categoryService.GetByIdAsync(objectId, CategoryResponseGroup.WithOutlines.ToString(), clone: false))?.Outlines,
            SeoExtensions.SeoProduct => (await _itemService.GetByIdAsync(objectId, ItemResponseGroup.WithOutlines.ToString(), clone: false))?.Outlines,
            _ => [],
        };

        if (outlines is null)
        {
            throw new InvalidOperationException($"{objectType} with ID '{objectId}' was not found.");
        }

        return outlines;
    }

    private static IList<Outline> CreateCatalogOutline(string catalogId, IList<SeoInfo> infos)
    {
        // For the catalog, we create a single outline with the catalog ID as the only item.
        return
        [
            new Outline
            {
                Items =
                [
                    new OutlineItem
                    {
                        Id = catalogId,
                        Name = catalogId,
                        SeoInfos = infos,
                        SeoObjectType = SeoExtensions.SeoCatalog,
                    }
                ]
            }
        ];
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

    protected virtual async Task<List<SeoInfo>> SearchSeoInfos(string permalink, Store store, string languageCode, bool isActive = true)
    {
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return [];
        }

        var slug = segments.Last();

        using var repository = _repositoryFactory();

        var entities = await repository.SeoInfos
            .Where(x =>
                x.IsActive == isActive &&
                x.Keyword == slug &&
                (x.Category != null && x.Category.IsActive || x.Item != null && x.Item.IsActive || x.Catalog != null) &&
                (string.IsNullOrEmpty(x.StoreId) || x.StoreId == store.Id) &&
                (string.IsNullOrEmpty(x.Language) || x.Language == languageCode || x.Language == store.DefaultLanguage))
            .ToListAsync();

        var categoryIds = entities.Select(x => x.CategoryId).Where(x => x != null).Distinct().ToArray();
        var seoList = new List<(string SeoPath, string OutlinePath, string Id)>();

        if (categoryIds.Length > 0)
        {
            var categories = (await _categoryService.GetByIdsAsync(categoryIds, $"{CategoryResponseGroup.WithOutlines},{CategoryResponseGroup.WithSeo}", store.Catalog))?.Where(x => (x.IsActive ?? true) && x.Outlines != null).ToArray();
            var seo = FilterByPermalink(categories);
            categoryIds = seo.Select(x => x.Id).Distinct().ToArray();
            seoList.AddRange(seo);
        }

        var itemIds = entities.Select(x => x.ItemId).Where(x => x != null).Distinct().ToArray();

        if (itemIds.Length > 0)
        {
            var items = (await _itemService.GetByIdsAsync(itemIds, $"{ItemResponseGroup.WithOutlines},{ItemResponseGroup.WithSeo}", store.Catalog))?.Where(x => (x.IsActive ?? true) && x.Outlines != null).ToArray();
            var seo = FilterByPermalink(items);
            itemIds = seo.Select(x => x.Id).Distinct().ToArray();
            seoList.AddRange(seo);
        }

        var result = entities.Where(x => x.CatalogId != null || categoryIds.Contains(x.CategoryId) || itemIds.Contains(x.ItemId)).ToArray();

        return result
            .Select(x =>
            {
                var item = x.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance());
                var outline = seoList.Where(s => s.Id == x.CategoryId || s.Id == x.ItemId).Select(s => s.OutlinePath).FirstOrDefault();
                item.Outline = outline;
                return item;
            })
            .OrderByDescending(GetScore)
            .ToList();

        // returns seo info from the given categories or products that match the permalink
        (string SeoPath, string OutlinePath, string Id)[] FilterByPermalink<T>(IEnumerable<T> elements) where T : IHasOutlines, ISeoSupport
        {
            return elements
                ?.SelectMany(x => x.Outlines.Select(o => new
                {
                    SeoPath = o.Items.GetSeoPath(store, store.DefaultLanguage),
                    OutlinePath = o.Items.GetOutlinePath(),
                    x.Id
                }))
                .Where(x => x.SeoPath == permalink)
                .Distinct()
                .Select(x => (x.SeoPath, x.OutlinePath, x.Id))
                .ToArray() ?? [];
        }

        int GetScore(SeoInfo seoInfo)
        {
            var score = 0;
            var hasLangCriteria = !string.IsNullOrEmpty(languageCode);

            if (seoInfo.StoreId.EqualsIgnoreCase(store.Id))
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
