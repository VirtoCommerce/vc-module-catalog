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
    public class CategorySearchService : SearchService<CategorySearchCriteria, CategorySearchResult, Category, CategoryEntity>, ICategorySearchService
    {
        public CategorySearchService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            ICategoryService categoryService)
            : base(catalogRepositoryFactory, platformMemoryCache, categoryService)
        {
        }

        [Obsolete("Use SearchAsync()")]
        public Task<CategorySearchResult> SearchCategoriesAsync(CategorySearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }


        protected override IList<SortInfo> BuildSortExpression(CategorySearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(CategoryEntity.Name) },
                };
            }

            return sortInfos;
        }

        protected override IQueryable<CategoryEntity> BuildQuery(IRepository repository, CategorySearchCriteria criteria)
        {
            var query = ((ICatalogRepository)repository).Categories;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }
            else if (!string.IsNullOrEmpty(criteria.Code))
            {
                query = query.Where(x => x.Code == criteria.Code);
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }

            if (!string.IsNullOrEmpty(criteria.CategoryId) && !criteria.SearchOnlyInRoot)
            {
                query = query.Where(x => x.ParentCategoryId == criteria.CategoryId);
            }

            if (criteria.SearchOnlyInRoot)
            {
                query = query.Where(x => x.ParentCategoryId == null);
            }

            return query;
        }
    }
}
