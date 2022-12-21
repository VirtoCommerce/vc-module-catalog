using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CatalogModule.Data.Services;

public class CategoryTreeService : ICategoryTreeService
{
    private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
    private readonly IPlatformMemoryCache _platformMemoryCache;
    private static readonly string _rootId = Guid.NewGuid().ToString();

    public CategoryTreeService(
        Func<ICatalogRepository> catalogRepositoryFactory,
        IPlatformMemoryCache platformMemoryCache)
    {
        _catalogRepositoryFactory = catalogRepositoryFactory;
        _platformMemoryCache = platformMemoryCache;
    }

    public async Task<IList<TreeNode>> GetNodesWithChildren(string catalogId, IList<string> ids, bool onlyActive)
    {
        var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetNodesWithChildren), catalogId, onlyActive.ToString());

        // GetOrLoadByIdsAsync() doesn't support null IDs, so we replace null with a unique ID here and restore it back to null later
        var models = await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, ReplaceNullId(ids),
            async missingIds =>
            {
                var result = await GetNodesWithChildrenNoCache(catalogId, RestoreNullId(missingIds), onlyActive);
                return ReplaceNullId(result).AsEnumerable();
            },
            ConfigureCache);

        return RestoreNullId(models);
    }


    protected virtual void ConfigureCache(MemoryCacheEntryOptions cacheOptions, string id, TreeNode model)
    {
        cacheOptions.AddExpirationToken(GenericSearchCachingRegion<Category>.CreateChangeToken());
    }

    protected virtual async Task<IList<TreeNode>> GetNodesWithChildrenNoCache(string catalogId, IList<string> ids, bool onlyActive)
    {
        using var repository = _catalogRepositoryFactory();

        // TODO: Implement batch loading for large number of IDs
        var childCategoriesQuery = GetChildCategoriesQuery(repository, catalogId, ids, onlyActive);
        var linkedCategoriesQuery = GetLinkedCategoriesQuery(repository, catalogId, ids, onlyActive);
        var query = childCategoriesQuery.Union(linkedCategoriesQuery);

        var relations = await query.ToArrayAsync();

        return relations
            .GroupBy(x => x.ParentId)
            .Select(g => new TreeNode
            {
                Id = g.Key,
                ChildIds = g.Select(x => x.ChildId).ToList()
            })
            .ToList();
    }

    protected virtual IQueryable<Relation> GetChildCategoriesQuery(ICatalogRepository repository, string catalogId, IList<string> parentIds, bool onlyActive)
    {
        var query = repository.Categories;

        if (parentIds.Any(string.IsNullOrEmpty))
        {
            query = query.Where(x => x.CatalogId == catalogId);
        }

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        query = parentIds.Count == 1
            ? query.Where(x => x.ParentCategoryId == parentIds.First())
            : query.Where(x => parentIds.Contains(x.ParentCategoryId));

        return query.Select(x => new Relation { ParentId = x.ParentCategoryId, ChildId = x.Id });
    }

    protected virtual IQueryable<Relation> GetLinkedCategoriesQuery(ICatalogRepository repository, string catalogId, IList<string> parentIds, bool onlyActive)
    {
        var query = repository.CategoryLinks.Where(x => x.TargetCatalogId == catalogId);

        if (onlyActive)
        {
            query = query.Where(x => x.SourceCategory.IsActive);
        }

        query = parentIds.Count == 1
            ? query.Where(x => x.TargetCategoryId == parentIds.First())
            : query.Where(x => parentIds.Contains(x.TargetCategoryId));

        return query.Select(x => new Relation { ParentId = x.TargetCategoryId, ChildId = x.SourceCategoryId });
    }

    protected class Relation
    {
        public string ParentId { get; init; }
        public string ChildId { get; init; }
    }

    protected virtual IList<string> ReplaceNullId(IList<string> ids)
    {
        if (ids.Count == 1 && ids.First() == null)
        {
            ids = new[] { _rootId };
        }

        return ids;
    }

    protected virtual IList<string> RestoreNullId(IList<string> ids)
    {
        if (ids.Count == 1 && ids.First() == _rootId)
        {
            ids = new string[] { null };
        }

        return ids;
    }

    protected virtual IList<TreeNode> ReplaceNullId(IList<TreeNode> nodes)
    {
        if (nodes.Count == 1 && nodes.First().Id == null)
        {
            nodes.First().Id = _rootId;
        }

        return nodes;
    }

    protected virtual IList<TreeNode> RestoreNullId(IList<TreeNode> nodes)
    {
        if (nodes.Count == 1 && nodes.First().Id == _rootId)
        {
            nodes.First().Id = null;
        }

        return nodes;
    }
}
