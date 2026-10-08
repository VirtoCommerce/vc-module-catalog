using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using VirtoCommerce.Seo.Core.Models.Explain;
using VirtoCommerce.Seo.Core.Services;
using VirtoCommerce.StoreModule.Core.Extensions;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants.SeoCandidateReasons;
using static VirtoCommerce.Seo.Core.ModuleConstants.CandidateReasons;
using static VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;
using SeoExtensions = VirtoCommerce.CatalogModule.Core.Extensions.SeoExtensions;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class CatalogSeoResolver : ISeoResolver
{
    private static readonly IEqualityComparer<(string ObjectType, string ObjectId)> _objectComparer = EqualityComparer<(string ObjectType, string ObjectId)>.Create(
        (x, y) => x.ObjectType.EqualsIgnoreCase(y.ObjectType) && x.ObjectId.EqualsIgnoreCase(y.ObjectId),
        x => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(x.ObjectType ?? string.Empty), StringComparer.OrdinalIgnoreCase.GetHashCode(x.ObjectId ?? string.Empty)));

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
        var candidates = await ResolveAsync(criteria, explain: false);

        return candidates
            .Where(x => x.IsResolved)
            .Select(x => x.SeoInfo)
            .ToList();
    }

    public virtual async Task<IList<SeoExplainItem>> GetCandidatesAsync(SeoSearchCriteria criteria)
    {
        var candidates = await ResolveAsync(criteria, explain: true);

        // FindSeoAsync decides what is resolved: a subclass may override it without touching the explanation
        var seoInfos = await FindSeoAsync(criteria);
        var unlistedIds = seoInfos.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            if (unlistedIds.Remove(candidate.SeoInfo.Id))
            {
                candidate.Reasons.Clear();
            }
            else if (candidate.IsResolved)
            {
                Reject(candidate, NotReturnedByResolver);
            }
        }

        return [.. candidates, .. seoInfos.Where(x => unlistedIds.Contains(x.Id)).Select(x => new SeoExplainItem(x))];
    }

    protected virtual async Task<IList<SeoExplainItem>> ResolveAsync(SeoSearchCriteria criteria, bool explain)
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

        var currentEntitySeoInfos = await SearchSeoInfos(permalink, store, criteria);
        var currentEntityCandidates = currentEntitySeoInfos.Select(x => new SeoExplainItem(x)).ToList();

        IList<SeoExplainItem> candidates = explain
            ? [.. currentEntityCandidates, .. await SearchRejectedSeoCandidates(permalink, store, criteria, currentEntitySeoInfos)]
            : currentEntityCandidates;

        if (currentEntityCandidates.Count == 0 || store.GetSeoLinksType() == SeoShort)
        {
            return candidates;
        }

        var groups = currentEntityCandidates.GroupBy(x => (x.SeoInfo.ObjectType, x.SeoInfo.ObjectId), _objectComparer).ToList();

        if (groups.Count == 1)
        {
            Reject(currentEntityCandidates.Skip(1), NotBestMatch);
            return candidates;
        }

        // We found multiple SEO records, need to choose the correct one by checking the parents recursively.
        var parentPermalink = string.Join('/', segments.SkipLast(1));
        var parentIds = await FindParentIds(parentPermalink, store, criteria);

        if (parentIds.Count == 0)
        {
            Reject(currentEntityCandidates, ParentNotResolved, parentPermalink);
            return candidates;
        }

        await RejectByParent(groups, parentIds, store, explain);

        return candidates;
    }

    private async Task<IList<string>> FindParentIds(string parentPermalink, Store store, SeoSearchCriteria criteria)
    {
        // It's not possible to resolve because we don't have parent segment
        if (parentPermalink.Length == 0)
        {
            return [store.Catalog];
        }

        var parentSearchCriteria = criteria.CloneTyped();
        parentSearchCriteria.Permalink = parentPermalink;
        var parentSeoInfos = await FindSeoAsync(parentSearchCriteria);

        return parentSeoInfos.Select(x => x.ObjectId).Distinct().ToList();
    }

    private async Task RejectByParent(IList<IGrouping<(string ObjectType, string ObjectId), SeoExplainItem>> groups, IList<string> parentIds, Store store, bool explain)
    {
        HashSet<SeoExplainItem> selectedCandidates = [];
        HashSet<SeoExplainItem> misplacedCandidates = [];

        foreach (var group in groups)
        {
            var outlines = await GetOutlines(group.Key.ObjectType, group.Key.ObjectId, group.Select(x => x.SeoInfo).ToList());

            if (!LongestOutlineContainsAnyParentId(outlines, store.Catalog, parentIds))
            {
                misplacedCandidates.UnionWith(group);
            }
            else if (selectedCandidates.Count == 0)
            {
                selectedCandidates.UnionWith(group);

                // Explain checks the remaining objects too, to tell a misplaced one from one that lost to the selected object
                if (!explain)
                {
                    break;
                }
            }
        }

        foreach (var candidate in groups.SelectMany(x => x).Where(x => !selectedCandidates.Contains(x)))
        {
            Reject(candidate, misplacedCandidates.Contains(candidate) ? ParentMismatch : NotBestMatch);
        }
    }

    private async Task<IList<Outline>> GetOutlines(string objectType, string objectId, IList<SeoInfo> infos)
    {
        var outlines = objectType switch
        {
            SeoExtensions.SeoCatalog => CreateCatalogOutline(objectId, infos),
            SeoExtensions.SeoCategory => (await _categoryService.GetByIdAsync(objectId, nameof(CategoryResponseGroup.WithOutlines), clone: false))?.Outlines,
            SeoExtensions.SeoProduct => (await _itemService.GetByIdAsync(objectId, nameof(ItemResponseGroup.WithOutlines), clone: false))?.Outlines,
            _ => [],
        };

        return outlines ?? throw new InvalidOperationException($"{objectType} with ID '{objectId}' was not found.");
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

    protected virtual async Task<List<SeoInfo>> SearchSeoInfos(string permalink, Store store, SeoSearchCriteria criteria, bool isActive = true)
    {
        var segments = permalink.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return [];
        }

        var slug = segments.Last();
        var entities = await GetSeoInfoEntities(slug, store, criteria, isActive);
        var seoPaths = await GetObjectSeoPaths(entities, store, criteria);
        List<SeoInfo> result = [];

        foreach (var entity in entities)
        {
            var seoPath = FindSeoPath(GetSeoPaths(seoPaths, entity), permalink);

            if (entity.CatalogId != null || seoPath.SeoPath != null)
            {
                result.Add(ToSeoInfo(entity, seoPath.OutlinePath));
            }
        }

        return result
            .OrderByDescending(x => GetSeoScore(x, store, criteria))
            .ToList();
    }

    protected virtual async Task<IList<SeoExplainItem>> SearchRejectedSeoCandidates(string permalink, Store store, SeoSearchCriteria criteria, IList<SeoInfo> resolvedSeoInfos)
    {
        using var repository = _repositoryFactory();
        var query = GetSeoCandidatesQuery(repository, permalink.Split('/', StringSplitOptions.RemoveEmptyEntries).Last());
        var resolvedIds = resolvedSeoInfos.Select(x => x.Id).ToList();
        var filters = GetSeoInfoFilters(store, criteria, isActive: true);

        // Records this store could use come first, so the Take limit never hides them; Id keeps the order stable
        var entities = await query
            .Where(x => !resolvedIds.Contains(x.Id))
            .OrderByDescending(filters[StoreMismatch])
            .ThenByDescending(filters[LanguageMismatch])
            .ThenBy(x => x.Id)
            .Take(criteria.Take)
            .ToListAsync();

        if (entities.Count == 0)
        {
            return [];
        }

        var ids = entities.Select(x => x.Id).ToList();
        var candidates = entities.Select(x => new SeoExplainItem(ToSeoInfo(x, outlinePath: null))).ToList();

        foreach (var (code, filter) in filters)
        {
            var passedIds = (await query.Where(x => ids.Contains(x.Id)).Where(filter).Select(x => x.Id).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Reject(candidates.Where(x => !passedIds.Contains(x.SeoInfo.Id)), code);
        }

        var seoPaths = await GetObjectSeoPaths(entities, store, criteria);

        foreach (var (entity, candidate) in entities.Zip(candidates))
        {
            var objectSeoPaths = GetSeoPaths(seoPaths, entity);

            if (entity.CatalogId == null && FindSeoPath(objectSeoPaths, permalink).SeoPath == null)
            {
                RejectBySeoPath(candidate, objectSeoPaths);
            }
        }

        return candidates;
    }

    protected virtual async Task<IList<SeoInfoEntity>> GetSeoInfoEntities(string slug, Store store, SeoSearchCriteria criteria, bool isActive)
    {
        using var repository = _repositoryFactory();
        return await GetSeoInfoQuery(repository, slug, store, criteria, isActive).ToListAsync();
    }

    protected virtual IQueryable<SeoInfoEntity> GetSeoInfoQuery(ICatalogRepository repository, string slug, Store store, SeoSearchCriteria criteria, bool isActive)
    {
        return GetSeoInfoFilters(store, criteria, isActive).Values
            .Aggregate(GetSeoCandidatesQuery(repository, slug), (query, filter) => query.Where(filter));
    }

    protected virtual IQueryable<SeoInfoEntity> GetSeoCandidatesQuery(ICatalogRepository repository, string slug)
    {
        return repository.SeoInfos.Where(x => x.Keyword == slug);
    }

    protected virtual IDictionary<string, Expression<Func<SeoInfoEntity, bool>>> GetSeoInfoFilters(Store store, SeoSearchCriteria criteria, bool isActive)
    {
        return new Dictionary<string, Expression<Func<SeoInfoEntity, bool>>>
        {
            [Inactive] = x => x.IsActive == isActive,
            [ObjectInactive] = x => x.Catalog != null || (x.Category != null ? x.Category.IsActive : x.Item != null && x.Item.IsActive),
            [StoreMismatch] = x => string.IsNullOrEmpty(x.StoreId) || x.StoreId == store.Id,
            [LanguageMismatch] = x => string.IsNullOrEmpty(x.Language) || x.Language == criteria.LanguageCode || x.Language == store.DefaultLanguage,
        };
    }

    protected static int GetSeoScore(SeoInfo seoInfo, Store store, SeoSearchCriteria criteria)
    {
        var score = 0;
        var hasLangCriteria = !string.IsNullOrEmpty(criteria.LanguageCode);

        if (seoInfo.StoreId.EqualsIgnoreCase(store.Id))
        {
            score += 2;
        }

        if (hasLangCriteria && seoInfo.LanguageCode.EqualsIgnoreCase(criteria.LanguageCode))
        {
            score += 1;
        }

        return score;
    }

    private async Task<Dictionary<string, (string SeoPath, string OutlinePath)[]>> GetObjectSeoPaths(IList<SeoInfoEntity> entities, Store store, SeoSearchCriteria criteria)
    {
        var result = new Dictionary<string, (string SeoPath, string OutlinePath)[]>(StringComparer.OrdinalIgnoreCase);

        var categoryIds = entities.Select(x => x.CategoryId).Where(x => x != null).Distinct().ToArray();

        if (categoryIds.Length > 0)
        {
            var categories = await _categoryService.GetByIdsAsync(categoryIds, $"{CategoryResponseGroup.WithOutlines},{CategoryResponseGroup.WithSeo}", store.Catalog);
            AddSeoPaths(categories, x => x.IsActive);
        }

        var itemIds = entities.Select(x => x.ItemId).Where(x => x != null).Distinct().ToArray();

        if (itemIds.Length > 0)
        {
            var items = await _itemService.GetByIdsAsync(itemIds, $"{ItemResponseGroup.WithOutlines},{ItemResponseGroup.WithSeo}", store.Catalog);
            AddSeoPaths(items, x => x.IsActive);
        }

        return result;

        void AddSeoPaths<T>(IEnumerable<T> elements, Func<T, bool?> isActive) where T : IHasOutlines, ISeoSupport
        {
            foreach (var element in (elements ?? []).Where(x => (isActive(x) ?? true) && x.Outlines != null))
            {
                result[element.Id] = element.Outlines
                    .Select(x => (x.Items.GetSeoPath(store, criteria.LanguageCode), x.Items.GetOutlinePath()))
                    .ToArray();
            }
        }
    }

    private static (string SeoPath, string OutlinePath)[] GetSeoPaths(Dictionary<string, (string SeoPath, string OutlinePath)[]> seoPaths, SeoInfoEntity entity)
    {
        return seoPaths.GetValueSafe(entity.CategoryId ?? entity.ItemId);
    }

    private static (string SeoPath, string OutlinePath) FindSeoPath((string SeoPath, string OutlinePath)[] objectSeoPaths, string permalink)
    {
        return objectSeoPaths?.FirstOrDefault(x => x.SeoPath == permalink) ?? default;
    }

    private static void RejectBySeoPath(SeoExplainItem candidate, (string SeoPath, string OutlinePath)[] objectSeoPaths)
    {
        var storeSeoPaths = objectSeoPaths?.Select(x => x.SeoPath).Where(x => x != null).Distinct().ToArray();

        if (objectSeoPaths == null)
        {
            Reject(candidate, ObjectInactive);
        }
        else if (objectSeoPaths.Length == 0)
        {
            Reject(candidate, NotInStoreCatalog);
        }
        else if (storeSeoPaths.Length == 0)
        {
            Reject(candidate, NoSeoPath);
        }
        else
        {
            Reject(candidate, PermalinkMismatch, string.Join(", ", storeSeoPaths));
        }
    }

    private static SeoInfo ToSeoInfo(SeoInfoEntity entity, string outlinePath)
    {
        var seoInfo = entity.ToModel(AbstractTypeFactory<SeoInfo>.TryCreateInstance());
        seoInfo.Outline = outlinePath;
        return seoInfo;
    }

    private static void Reject(IEnumerable<SeoExplainItem> candidates, string code, string details = null)
    {
        foreach (var candidate in candidates)
        {
            Reject(candidate, code, details);
        }
    }

    private static void Reject(SeoExplainItem candidate, string code, string details = null)
    {
        if (candidate.Reasons.All(x => x.Code != code))
        {
            candidate.Reasons.Add(new SeoCandidateReason(code, details));
        }
    }
}
