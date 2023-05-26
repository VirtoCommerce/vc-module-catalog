using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchService : SearchService<ProductSearchCriteria, ProductSearchResult, CatalogProduct, ItemEntity>, IProductSearchService
    {
        private readonly IItemService _itemService;

        public ProductSearchService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IItemService itemService)
            : base(catalogRepositoryFactory, platformMemoryCache, itemService)
        {
            _itemService = itemService;
        }

        [Obsolete("Use SearchAsync()")]
        public Task<ProductSearchResult> SearchProductsAsync(ProductSearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        public override Task<ProductSearchResult> SearchAsync(ProductSearchCriteria criteria)
        {
            return InternalSearchAsync(criteria, clone: true);
        }

        /// <summary>
        /// Returns data from the cache without cloning. This consumes less memory, but returned data must not be modified.
        /// </summary>
        public Task<ProductSearchResult> SearchNoCloneAsync(ProductSearchCriteria criteria)
        {
            return InternalSearchAsync(criteria, clone: false);
        }


        protected virtual async Task<ProductSearchResult> InternalSearchAsync(ProductSearchCriteria criteria, bool clone)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(InternalSearchAsync), criteria.GetCacheKey());

            var idsResult = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheOptions =>
            {
                cacheOptions.AddExpirationToken(CreateCacheToken(criteria));
                return await SearchIdsNoCacheAsync(criteria);
            });

            var result = AbstractTypeFactory<ProductSearchResult>.TryCreateInstance();
            result.TotalCount = idsResult.TotalCount;

            if (idsResult.Results.IsNullOrEmpty())
            {
                result.Results = Array.Empty<CatalogProduct>();
            }
            else
            {
                IEnumerable<CatalogProduct> products = clone
                    ? await _itemService.GetAsync(idsResult.Results.ToList(), criteria.ResponseGroup)
                    : await _itemService.GetNoCloneAsync(idsResult.Results, criteria.ResponseGroup);

                result.Results = products
                    .OrderBy(x => idsResult.Results.IndexOf(x.Id))
                    .ToList();
            }

            return await ProcessSearchResultAsync(result, criteria);
        }

        protected virtual IChangeToken CreateCacheToken(ProductSearchCriteria criteria)
        {
            return GenericSearchCachingRegion<CatalogProduct>.CreateChangeToken();
        }

        protected virtual async Task<GenericSearchResult<string>> SearchIdsNoCacheAsync(ProductSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<GenericSearchResult<string>>.TryCreateInstance();
            result.Results = Array.Empty<string>();

            using var repository = _repositoryFactory();
            var query = BuildQuery(repository, criteria);
            var needExecuteCount = criteria.Take == 0;

            if (criteria.Take > 0)
            {
                result.Results = await query
                    .OrderBySortInfos(BuildSortExpression(criteria))
                    .ThenBy(x => x.Id)
                    .Select(x => x.Id)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .ToArrayAsync();

                result.TotalCount = result.Results.Count;

                // This reduces a load of a relational database by skipping count query in case of:
                // - First page is reading (Skip is 0)
                // - Count in reading result less than Take value.
                if (criteria.Skip > 0 || result.TotalCount == criteria.Take)
                {
                    needExecuteCount = true;
                }
            }

            if (needExecuteCount)
            {
                result.TotalCount = await query.CountAsync();
            }

            return result;
        }

        protected override IQueryable<ItemEntity> BuildQuery(IRepository repository, ProductSearchCriteria criteria)
        {
            var catalogRepository = (ICatalogRepository)repository;
            var query = catalogRepository.Items;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                var searchCategoryIds = criteria.CategoryIds;

                if (criteria.SearchInChildren)
                {
                    searchCategoryIds = searchCategoryIds.Concat(catalogRepository.GetAllChildrenCategoriesIdsAsync(searchCategoryIds).GetAwaiter().GetResult()).ToArray();
                }

                query = query.Where(x => searchCategoryIds.Contains(x.CategoryId) || x.CategoryLinks.Any(link => searchCategoryIds.Contains(link.CategoryId)));
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }

            if (!criteria.OuterIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.OuterIds.Contains(x.OuterId));
            }

            if (!criteria.Skus.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Skus.Contains(x.Code));
            }
            if (!string.IsNullOrEmpty(criteria.MainProductId))
            {
                query = query.Where(x => x.ParentId == criteria.MainProductId);
            }
            else if (!criteria.SearchInVariations)
            {
                query = query.Where(x => x.ParentId == null);
            }

            if (!criteria.PropertyName.IsNullOrEmpty())
            {
                query = query.Where(x => x.ItemPropertyValues.Any(v => v.Name == criteria.PropertyName));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(ProductSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(ItemEntity.Name) },
                };
            }

            return sortInfos;
        }
    }
}
