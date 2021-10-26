using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule2.Data.Search
{
    public class CategorySearchService2 : CategorySearchService
    {
        public CategorySearchService2(Func<ICatalogRepository> catalogRepositoryFactory, ICategoryService categoryService) : base(catalogRepositoryFactory, categoryService)
        {
        }
        protected override IQueryable<CategoryEntity> BuildQuery(ICatalogRepository repository, CategorySearchCriteria criteria)
        {
            return base.BuildQuery(repository, criteria);
        }
        protected override IList<SortInfo> BuildSortExpression(CategorySearchCriteria criteria)
        {
            return base.BuildSortExpression(criteria);
        }
    }
}
