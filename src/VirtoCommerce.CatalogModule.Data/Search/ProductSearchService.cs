using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchService : SearchService<ProductSearchCriteria, ProductSearchResult, CatalogProduct, ItemEntity>, IProductSearchService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IItemService _itemService;
        public ProductSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IPlatformMemoryCache platformMemoryCache, IItemService itemService)
         : base(catalogRepositoryFactory, platformMemoryCache, (ICrudService<CatalogProduct>)itemService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _itemService = itemService;
        }

        public Task<ProductSearchResult> SearchProductsAsync(ProductSearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        public override async Task<ProductSearchResult> SearchAsync(ProductSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<ProductSearchResult>.TryCreateInstance();

            using (var repository = _catalogRepositoryFactory())
            {
                //Optimize performance and CPU usage
                repository.DisableChangesTracking();

                var sortInfos = BuildSortExpression(criteria);
                var query = BuildQuery(repository, criteria);

                result.TotalCount = await query.CountAsync();
                if (criteria.Take > 0 && result.TotalCount > 0)
                {
                    var ids = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                        .Select(x => x.Id)
                                        .Skip(criteria.Skip).Take(criteria.Take)
                                        .AsNoTracking()
                                        .ToArrayAsync();

                    result.Results = (await _itemService.GetByIdsAsync(ids, criteria.ResponseGroup)).OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();
                }
            }
            return result;
        }

        protected override IQueryable<ItemEntity> BuildQuery(IRepository repository, ProductSearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).Items;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                var searchCategoryIds = criteria.CategoryIds;

                if (criteria.SearchInChildren)
                {
                    searchCategoryIds = searchCategoryIds.Concat(((ICatalogRepository)repository).GetAllChildrenCategoriesIdsAsync(searchCategoryIds).GetAwaiter().GetResult()).ToArray();
                }

                query = query.Where(x => searchCategoryIds.Contains(x.CategoryId) || x.CategoryLinks.Any(link => searchCategoryIds.Contains(link.CategoryId)));
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
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
                query = query.Where(x => x.ItemPropertyValues.Any(x => x.Name == criteria.PropertyName));
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
                    new SortInfo { SortColumn = nameof(ItemEntity.Name) }
                };
            }

            return sortInfos;
        }
    }
}
