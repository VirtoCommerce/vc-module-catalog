using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Web.Search
{
    public class CategorySearchService2 : CategorySearchService
    {
        public CategorySearchService2(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            ICategoryService categoryService)
            : base(catalogRepositoryFactory, platformMemoryCache, categoryService)
        {
        }

        protected override IQueryable<CategoryEntity> BuildQuery(IRepository repository, CategorySearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }

        protected override IList<SortInfo> BuildSortExpression(CategorySearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
