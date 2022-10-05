using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogModule.Data.Search
{
    public class ProductSearchService : SearchService<ProductSearchCriteria, ProductSearchResult, CatalogProduct, ItemEntity>, IProductSearchService
    {
        public ProductSearchService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IItemService itemService)
            : base(catalogRepositoryFactory, platformMemoryCache, itemService)
        {
        }

        [Obsolete("Use SearchAsync()")]
        public Task<ProductSearchResult> SearchProductsAsync(ProductSearchCriteria criteria)
        {
            return SearchAsync(criteria);
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
