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
    public class CategorySearchService : SearchService<CategorySearchCriteria, CategorySearchResult, Category, CategoryEntity>, ICategorySearchService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly ICategoryService _categoryService;
        public CategorySearchService(Func<ICatalogRepository> catalogRepositoryFactory, IPlatformMemoryCache platformMemoryCache, ICategoryService categoryService)
            : base(catalogRepositoryFactory, platformMemoryCache, (ICrudService<Category>)categoryService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _categoryService = categoryService;
        }

        public Task<CategorySearchResult> SearchCategoriesAsync(CategorySearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        public override async Task<CategorySearchResult> SearchAsync(CategorySearchCriteria criteria)
        {
            var result = AbstractTypeFactory<CategorySearchResult>.TryCreateInstance();

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
                                        .ToArrayAsync();

                    result.Results = (await _categoryService.GetByIdsAsync(ids, criteria.ResponseGroup)).OrderBy(x => Array.IndexOf(ids, x.Id)).ToList();
                }
            }

            return result;
        }

        protected override IList<SortInfo> BuildSortExpression(CategorySearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(CategoryEntity.Name) }
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
